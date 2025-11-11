using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace InfernumSOTSSanctuaryAdjuster
{
    public class SOTSSanctuaryHookSystem : ModSystem
    {
        private ILHook _ilHook;
        private static FieldInfo _fiSpawnPos, _fiRectangle;
        private static MethodInfo _miSetRect;

        public override void Load()
        {
            if (!ModLoader.TryGetMod("SOTS", out var sots) || sots is null)
            {
                return;
            }

            var asm = sots.GetType().Assembly;

            var helperT =
                asm.GetType("SOTS.WorldgenHelpers.SanctuaryWorldgenHelper", throwOnError: false)
                ?? asm.GetTypes().FirstOrDefault(t => t.Name == "SanctuaryWorldgenHelper");

            if (helperT == null) { return; }

            _fiSpawnPos = helperT.GetField("SpawnPos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            _fiRectangle = helperT.GetField("Rectangle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            _miSetRect = helperT.GetMethod("SetRect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            var miGen = helperT.GetMethod("GenerateSanctuary", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (miGen == null) {  return; }

            _ilHook = new ILHook(miGen, Patch_GenerateSanctuary);
            ModContent.GetInstance<InfernumSOTSSanctuaryAdjuster>().Logger.Info("Sanctuary IL patch installed.");
        }

        public override void Unload()
        {
            _ilHook?.Dispose();
            _ilHook = null;
            _fiSpawnPos = _fiRectangle = null;
            _miSetRect = null;
        }

        private void Patch_GenerateSanctuary(ILContext il)
        {
            var c = new ILCursor(il);

            // Insert AFTER InitTypes() call
            if (!c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(out var m) && m.Name == "InitTypes"))
            {
                return;
            }

            c.EmitDelegate<Action>(() =>
            {
                try
                {
                    var cfg = ModContent.GetInstance<SanctuarySpawnConfig>();

                    bool compat =
                        (!cfg.OnlyWhenSpookyOrInfernum) ||
                        ModLoader.TryGetMod("Spooky", out _) ||
                        ModLoader.TryGetMod("InfernumMode", out _);

                    if (!compat)
                    {
                        ModContent.GetInstance<InfernumSOTSSanctuaryAdjuster>().Logger.Info("Compat not active; leaving SOTS position.");
                        return;
                    }

                    int maxX = Main.maxTilesX;
                    int center = maxX / 2;
                    int margin = Math.Clamp(cfg.MinMarginTiles, 0, center - 1);
                    bool jungleRight = GenVars.JungleX > center;

                    int pos;
                    switch (cfg.PositionMode)
                    {
                        case PositionMode.Fraction:
                            pos = (int)Math.Round((jungleRight ? cfg.LeftFraction : cfg.RightFraction) * maxX);
                            break;

                        case PositionMode.Tiles:
                            pos = jungleRight ? cfg.LeftTiles : (maxX - cfg.RightTilesFromRightEdge);
                            break;

                        case PositionMode.Nudge:
                        default:
                            {
                                int basePos = jungleRight ? (maxX * 1) / 8 : (maxX * 7) / 8;
                                int nudge = Math.Max(0, cfg.NudgeTilesTowardCenter);
                                pos = basePos + Math.Sign(center - basePos) * nudge;
                                break;
                            }
                    }

                    pos = Math.Clamp(pos, margin, maxX - margin);

                    _fiSpawnPos?.SetValue(null, pos);
                    if (_miSetRect != null && _fiRectangle != null)
                    {
                        Rectangle rect = (Rectangle)_miSetRect.Invoke(null, null);
                        _fiRectangle.SetValue(null, rect);
                    }

                    ModContent.GetInstance<InfernumSOTSSanctuaryAdjuster>().Logger.Info($"Sanctuary position forced to {pos} (maxX={maxX}, jungleRight={jungleRight}).");
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<InfernumSOTSSanctuaryAdjuster>().Logger.Error($"Sanctuary patch delegate failed: {ex}");
                }
            });
        }
    }
}

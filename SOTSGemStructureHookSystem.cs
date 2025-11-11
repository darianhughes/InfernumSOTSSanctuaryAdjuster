using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace InfernumSOTSSanctuaryAdjuster
{
    public class SOTSGemStructureHookSystem : ModSystem
    {
        private ILHook _placeAndGenerateSapphireHook;

        public override void Load()
        {
            if (!ModLoader.TryGetMod("SOTS", out var sots))
                return;

            var helperType = sots.Code?.GetType("SOTS.WorldgenHelpers.GemStructureWorldgenHelper");
            if (helperType == null)
                return;

            var method = helperType.GetMethod("PlaceAndGenerateSapphire",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (method == null)
                return;

            _placeAndGenerateSapphireHook = new ILHook(method, PatchPlaceAndGenerateSapphire);
        }

        public override void Unload()
        {
            _placeAndGenerateSapphireHook?.Dispose();
            _placeAndGenerateSapphireHook = null;
        }

        private static void PatchPlaceAndGenerateSapphire(ILContext il)
        {
            var c = new ILCursor(il);

            int num5Index = -1;

            if (!c.TryGotoNext(instr =>
            {
                if (!instr.MatchStloc(out int tmpIndex))
                    return false;

                // Scan a few instructions backwards for ldc.i4.1 / ldc.i4.m1
                var prev = instr.Previous;
                for (int k = 0; k < 8 && prev != null; k++, prev = prev.Previous)
                {
                    if (prev.OpCode == OpCodes.Ldc_I4_1 || prev.OpCode == OpCodes.Ldc_I4_M1)
                    {
                        num5Index = tmpIndex;
                        return true;
                    }
                }

                return false;
            }))
            {
                return;
            }

            if (num5Index < 0)
                return;

            c.Index++;
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Stloc, num5Index);
        }
    }
}

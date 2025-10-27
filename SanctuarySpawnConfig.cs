using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace InfernumSOTSSanctuaryAdjuster
{
    public class SanctuarySpawnConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("General")]
        [Label("Only apply when Spooky or Infernum is enabled")]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool OnlyWhenSpookyOrInfernum { get; set; } = true;

        [Label("Position Mode")]
        [DefaultValue(PositionMode.Fraction)]
        [ReloadRequired]
        public PositionMode PositionMode { get; set; } = PositionMode.Fraction;

        [Label("Minimum margin from world edges (tiles)")]
        [Range(0, 5000)]
        [DefaultValue(300)]
        [ReloadRequired]
        public int MinMarginTiles { get; set; } = 300;

        [Header("Fraction")]
        [Label("Left-side fraction (when Jungle is on the RIGHT)")]
        [Range(0f, 1f)]
        [DefaultValue(0.27f)]
        [ReloadRequired]
        public float LeftFraction { get; set; } = 0.25f;

        [Label("Right-side fraction (when Jungle is on the LEFT)")]
        [Range(0f, 1f)]
        [DefaultValue(0.73f)]
        [ReloadRequired]
        public float RightFraction { get; set; } = 0.75f;

        [Header("Tiles")]
        [Label("Absolute tiles from left (when Jungle is on the RIGHT)")]
        [Range(0, 100000)]
        [DefaultValue(1200)]
        [ReloadRequired]
        public int LeftTiles { get; set; } = 1200;

        [Label("Absolute tiles from RIGHT edge (when Jungle is on the LEFT)")]
        [Range(0, 100000)]
        [DefaultValue(1200)]
        [ReloadRequired]
        public int RightTilesFromRightEdge { get; set; } = 1200;

        [Header("Nudge")]
        [Label("Nudge toward center (tiles)")]
        [Range(0, 10000)]
        [DefaultValue(600)]
        [ReloadRequired]
        public int NudgeTilesTowardCenter { get; set; } = 600;
    }

    public enum PositionMode
    {
        Fraction,   // Use LeftFraction / RightFraction of world width
        Tiles,      // Use LeftTiles / (worldWidth - RightTilesFromRightEdge)
        Nudge       // Start from SOTS’s original and move toward center by NudgeTilesTowardCenter
    }
}

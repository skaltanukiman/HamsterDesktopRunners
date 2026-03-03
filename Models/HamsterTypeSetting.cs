using System.Collections.Generic;

namespace HamsterDesktopRunners.Models
{
    public class HamsterTypeSetting
    {
        public HamsterType Type { get; set; }
        public List<BehaviorSetting> Behaviors { get; set; } = new List<BehaviorSetting>();

        /// <summary>左向き時の種X座標オフセット（ハムスター位置からの相対値）</summary>
        public int SeedOffsetLeftX { get; set; } = 30;

        /// <summary>右向き時の種X座標オフセット（ImageWidthからの相対値）</summary>
        public int SeedOffsetRightX { get; set; } = -50;

        /// <summary>種Y座標オフセット（ImageHeightからの相対値）</summary>
        public int SeedOffsetY { get; set; } = -24;
    }
}

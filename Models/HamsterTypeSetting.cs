using System.Collections.Generic;

namespace HamsterDesktopRunners.Models
{
    public class HamsterTypeSetting
    {
        public HamsterType Type { get; set; }
        public List<BehaviorSetting> Behaviors { get; set; } = new List<BehaviorSetting>();
    }
}

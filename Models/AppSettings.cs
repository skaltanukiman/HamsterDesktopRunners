using System.Collections.Generic;

namespace HamsterDesktopRunners.Models
{
    public class AppSettings
    {
        public List<HamsterTypeSetting> HamsterSettings { get; set; } = new List<HamsterTypeSetting>();

        /// <summary>
        /// デフォルト設定を生成する。
        /// 全ハムスター種別に対してMove(ON), Eat(ON)を設定する。
        /// </summary>
        public static AppSettings CreateDefault()
        {
            var settings = new AppSettings();
            foreach (HamsterType type in System.Enum.GetValues(typeof(HamsterType)))
            {
                settings.HamsterSettings.Add(new HamsterTypeSetting
                {
                    Type = type,
                    Behaviors = new List<BehaviorSetting>
                    {
                        new BehaviorSetting { BehaviorId = "move", IsEnabled = true },
                        new BehaviorSetting { BehaviorId = "eat",  IsEnabled = true },
                    }
                });
            }
            return settings;
        }

        /// <summary>
        /// 指定種別の指定BehaviorIdの設定を返す。なければデフォルト(有効)を返す。
        /// </summary>
        public bool IsBehaviorEnabled(HamsterType type, string behaviorId)
        {
            var typeSetting = HamsterSettings.Find(s => s.Type == type);
            if (typeSetting == null) return true;
            var bSetting = typeSetting.Behaviors.Find(b => b.BehaviorId == behaviorId);
            return bSetting?.IsEnabled ?? true;
        }

        /// <summary>
        /// 指定種別のひまわりの種オフセット値を返す。設定がなければデフォルト値を返す。
        /// </summary>
        public (int leftX, int rightX, int y) GetSeedOffset(HamsterType type)
        {
            var typeSetting = HamsterSettings.Find(s => s.Type == type);
            if (typeSetting == null)
                return (30, -50, -24);
            return (typeSetting.SeedOffsetLeftX, typeSetting.SeedOffsetRightX, typeSetting.SeedOffsetY);
        }
    }
}

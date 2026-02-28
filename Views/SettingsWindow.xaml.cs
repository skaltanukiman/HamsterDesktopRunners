using System.Windows;
using HamsterDesktopRunners.Models;
using HamsterDesktopRunners.Services;

namespace HamsterDesktopRunners.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ISettingsRepository _repo;
        private AppSettings _settings;
        private bool _loading = false;

        public SettingsWindow(ISettingsRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            _settings = _repo.Load();
            LoadToUI();
        }

        private void LoadToUI()
        {
            _loading = true;
            ChkGoldenMove.IsChecked      = _settings.IsBehaviorEnabled(HamsterType.Golden,     "move");
            ChkGoldenEat.IsChecked       = _settings.IsBehaviorEnabled(HamsterType.Golden,     "eat");
            ChkDjungarianMove.IsChecked  = _settings.IsBehaviorEnabled(HamsterType.Djungarian, "move");
            ChkDjungarianEat.IsChecked   = _settings.IsBehaviorEnabled(HamsterType.Djungarian, "eat");
            _loading = false;
        }

        private void BehaviorCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            SaveFromUI();
        }

        private void SaveFromUI()
        {
            SetBehavior(HamsterType.Golden,     "move", ChkGoldenMove.IsChecked == true);
            SetBehavior(HamsterType.Golden,     "eat",  ChkGoldenEat.IsChecked  == true);
            SetBehavior(HamsterType.Djungarian, "move", ChkDjungarianMove.IsChecked == true);
            SetBehavior(HamsterType.Djungarian, "eat",  ChkDjungarianEat.IsChecked  == true);
        }

        private void SetBehavior(HamsterType type, string behaviorId, bool enabled)
        {
            var typeSetting = _settings.HamsterSettings.Find(s => s.Type == type);
            if (typeSetting == null)
            {
                typeSetting = new HamsterTypeSetting { Type = type };
                _settings.HamsterSettings.Add(typeSetting);
            }
            var bSetting = typeSetting.Behaviors.Find(b => b.BehaviorId == behaviorId);
            if (bSetting == null)
            {
                bSetting = new BehaviorSetting { BehaviorId = behaviorId };
                typeSetting.Behaviors.Add(bSetting);
            }
            bSetting.IsEnabled = enabled;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFromUI();
            _repo.Save(_settings);
            Close();
        }
    }
}

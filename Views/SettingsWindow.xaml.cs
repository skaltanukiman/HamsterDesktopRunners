using System.Windows;
using HamsterDesktopRunners.Models;
using HamsterDesktopRunners.Services;

namespace HamsterDesktopRunners.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ISettingsRepository _repo;
        private AppSettings _settings;
        private bool _loading = true; // InitializeComponent中のイベント発火を抑制

        public SettingsWindow(ISettingsRepository repo)
        {
            InitializeComponent();
            _repo = repo;
            _settings = _repo.Load();
            LoadToUI(); // ここで_loading = falseになる
        }

        private void LoadToUI()
        {
            _loading = true;
            // Behavior チェックボックス
            ChkGoldenMove.IsChecked      = _settings.IsBehaviorEnabled(HamsterType.Golden,     "move");
            ChkGoldenEat.IsChecked       = _settings.IsBehaviorEnabled(HamsterType.Golden,     "eat");
            ChkDjungarianMove.IsChecked  = _settings.IsBehaviorEnabled(HamsterType.Djungarian, "move");
            ChkDjungarianEat.IsChecked   = _settings.IsBehaviorEnabled(HamsterType.Djungarian, "eat");

            // ひまわりの種オフセット
            var goldenOffset = _settings.GetSeedOffset(HamsterType.Golden);
            SldGoldenLeftX.Value  = goldenOffset.leftX;
            SldGoldenRightX.Value = goldenOffset.rightX;
            SldGoldenY.Value      = goldenOffset.y;

            var djungarianOffset = _settings.GetSeedOffset(HamsterType.Djungarian);
            SldDjungarianLeftX.Value  = djungarianOffset.leftX;
            SldDjungarianRightX.Value = djungarianOffset.rightX;
            SldDjungarianY.Value      = djungarianOffset.y;

            UpdateSeedLabels();
            _loading = false;
        }

        private void UpdateSeedLabels()
        {
            LblGoldenLeftX.Text  = ((int)SldGoldenLeftX.Value).ToString();
            LblGoldenRightX.Text = ((int)SldGoldenRightX.Value).ToString();
            LblGoldenY.Text      = ((int)SldGoldenY.Value).ToString();

            LblDjungarianLeftX.Text  = ((int)SldDjungarianLeftX.Value).ToString();
            LblDjungarianRightX.Text = ((int)SldDjungarianRightX.Value).ToString();
            LblDjungarianY.Text      = ((int)SldDjungarianY.Value).ToString();
        }

        private void BehaviorCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            SaveFromUI();
        }

        private void SeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            UpdateSeedLabels();
            SaveFromUI();
        }

        private void SaveFromUI()
        {
            SetBehavior(HamsterType.Golden,     "move", ChkGoldenMove.IsChecked == true);
            SetBehavior(HamsterType.Golden,     "eat",  ChkGoldenEat.IsChecked  == true);
            SetBehavior(HamsterType.Djungarian, "move", ChkDjungarianMove.IsChecked == true);
            SetBehavior(HamsterType.Djungarian, "eat",  ChkDjungarianEat.IsChecked  == true);

            // 種オフセット
            SetSeedOffset(HamsterType.Golden,     (int)SldGoldenLeftX.Value,     (int)SldGoldenRightX.Value,     (int)SldGoldenY.Value);
            SetSeedOffset(HamsterType.Djungarian, (int)SldDjungarianLeftX.Value, (int)SldDjungarianRightX.Value, (int)SldDjungarianY.Value);
        }

        private void SetBehavior(HamsterType type, string behaviorId, bool enabled)
        {
            var typeSetting = GetOrCreateTypeSetting(type);
            var bSetting = typeSetting.Behaviors.Find(b => b.BehaviorId == behaviorId);
            if (bSetting == null)
            {
                bSetting = new BehaviorSetting { BehaviorId = behaviorId };
                typeSetting.Behaviors.Add(bSetting);
            }
            bSetting.IsEnabled = enabled;
        }

        private void SetSeedOffset(HamsterType type, int leftX, int rightX, int y)
        {
            var typeSetting = GetOrCreateTypeSetting(type);
            typeSetting.SeedOffsetLeftX  = leftX;
            typeSetting.SeedOffsetRightX = rightX;
            typeSetting.SeedOffsetY      = y;
        }

        private HamsterTypeSetting GetOrCreateTypeSetting(HamsterType type)
        {
            var typeSetting = _settings.HamsterSettings.Find(s => s.Type == type);
            if (typeSetting == null)
            {
                typeSetting = new HamsterTypeSetting { Type = type };
                _settings.HamsterSettings.Add(typeSetting);
            }
            return typeSetting;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFromUI();
            _repo.Save(_settings);
            Close();
        }
    }
}

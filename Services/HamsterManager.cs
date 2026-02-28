using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using HamsterDesktopRunners.Behaviors;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Services
{
    public class HamsterManager
    {
        public List<Hamster> Hamsters { get; } = new List<Hamster>();
        private readonly DispatcherTimer _timer;
        private static readonly Random Rnd = new Random();

        public List<Rect> ScreenBoundsList { get; set; } = new List<Rect>();
        public HamsterType SelectedType { get; set; } = HamsterType.Golden;

        /// <summary>種別ごとのアクティブBehaviorリスト</summary>
        private readonly Dictionary<HamsterType, List<IHamsterBehavior>> _behaviors = new();

        private readonly ISettingsRepository _settingsRepo;
        private AppSettings _settings;

        public event EventHandler? HamstersUpdated;

        public HamsterManager(List<Rect> screenBoundsList, ISettingsRepository settingsRepo)
        {
            ScreenBoundsList = screenBoundsList;
            _settingsRepo = settingsRepo;
            _settings = _settingsRepo.Load();

            // 各種別のBehaviorリストを構築
            RebuildBehaviors();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>設定に基づいてBehaviorリストを再構築する。設定変更後に呼ぶ。</summary>
        public void RebuildBehaviors()
        {
            _settings = _settingsRepo.Load();
            _behaviors.Clear();

            foreach (HamsterType type in Enum.GetValues(typeof(HamsterType)))
            {
                var list = new List<IHamsterBehavior>
                {
                    new EatBehavior  { IsEnabled = _settings.IsBehaviorEnabled(type, "eat")  },
                    new MoveBehavior { IsEnabled = _settings.IsBehaviorEnabled(type, "move") },
                };
                _behaviors[type] = list;
            }
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        public void AddHamster()
        {
            if (ScreenBoundsList.Count == 0) return;
            var bounds = ScreenBoundsList[0];
            double startX = bounds.Left + Rnd.NextDouble() * Math.Max(50, bounds.Width - 60);
            double startY = bounds.Top  + Rnd.NextDouble() * Math.Max(50, bounds.Height - 60);

            var behaviors = _behaviors.TryGetValue(SelectedType, out var b) ? b : new List<IHamsterBehavior>();
            Hamsters.Add(new Hamster(startX, startY, SelectedType, behaviors));
        }

        public void ClearHamsters()
        {
            Hamsters.Clear();
            HamstersUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var hamster in Hamsters)
                hamster.Update(ScreenBoundsList);

            HamstersUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}

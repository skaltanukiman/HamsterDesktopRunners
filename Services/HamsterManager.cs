using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Services
{
    public class HamsterManager
    {
        public List<Hamster> Hamsters { get; } = new List<Hamster>();
        private DispatcherTimer _timer;
        private static readonly Random Rnd = new Random();
        public List<Rect> ScreenBoundsList { get; set; } = new List<Rect>();
        public HamsterType SelectedType { get; set; } = HamsterType.Golden;

        public event EventHandler? HamstersUpdated;

        public HamsterManager(List<Rect> screenBoundsList)
        {
            ScreenBoundsList = screenBoundsList;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 約60FPS
            };
            _timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void AddHamster()
        {
            if (ScreenBoundsList.Count == 0) return;
            var bounds = ScreenBoundsList[0];
            // 画面内のランダムな位置に追加
            double startX = bounds.Left + Rnd.NextDouble() * (Math.Max(50, bounds.Width - 60));
            double startY = bounds.Top + Rnd.NextDouble() * (Math.Max(50, bounds.Height - 60));
            Hamsters.Add(new Hamster(startX, startY, SelectedType));
        }

        public void ClearHamsters()
        {
            Hamsters.Clear();
            HamstersUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var hamster in Hamsters)
            {
                hamster.Update(ScreenBoundsList);
            }
            HamstersUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}

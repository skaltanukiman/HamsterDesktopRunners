using System;
using System.Collections.Generic;
using System.Windows;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Behaviors
{
    /// <summary>
    /// 食事行動。Runningからランダムに発動し、その場で停止して咀嚼アニメーションを行う。
    /// </summary>
    public class EatBehavior : IHamsterBehavior
    {
        public string Id => "eat";
        public string DisplayName => "食事 (Eat)";
        public bool IsEnabled { get; set; } = true;

        private static readonly Random Rnd = new Random();
        private const double EatProbability = 0.12;
        private const int EatDuration = 120;

        public void Update(Hamster hamster, List<Rect> screens)
        {
            if (!IsEnabled) return;

            if (hamster.CurrentState == Hamster.State.Eating)
            {
                hamster.EatTimer--;
                hamster.FrameCounter++;
                if (hamster.FrameCounter >= 15)
                {
                    hamster.FrameCounter = 0;
                    hamster.CurrentFrame = hamster.CurrentFrame == 0 ? 1 : 0;
                }

                if (hamster.EatTimer <= 0)
                {
                    hamster.CurrentState = Hamster.State.Running;
                    hamster.StateTimer = Rnd.Next(80, 200);
                    double speed = Rnd.NextDouble() * 3.0 + 1.0;
                    double angle = Rnd.NextDouble() * Math.PI * 2;
                    hamster.Velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
                    hamster.CurrentDirection = hamster.Velocity.X < 0 ? Hamster.Direction.Left : Hamster.Direction.Right;
                }
                return;
            }

            if (hamster.CurrentState == Hamster.State.Running && hamster.StateTimer <= 0)
            {
                if (Rnd.NextDouble() < EatProbability)
                {
                    hamster.CurrentState = Hamster.State.Eating;
                    hamster.EatTimer = EatDuration;
                    hamster.Velocity = new Vector(0, 0);
                    hamster.CurrentFrame = 0;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Behaviors
{
    /// <summary>
    /// 通常移動行動。移動・境界反射・アイドル遷移ロジックを担当する。
    /// </summary>
    public class MoveBehavior : IHamsterBehavior
    {
        public string Id => "move";
        public string DisplayName => "通常移動";
        public bool IsEnabled { get; set; } = true;

        private static readonly Random Rnd = new Random();

        public void Update(Hamster hamster, List<Rect> screens)
        {
            if (!IsEnabled) return;
            if (screens == null || screens.Count == 0) return;

            // Eating中は移動しない
            if (hamster.CurrentState == Hamster.State.Eating) return;

            hamster.StateTimer--;
            if (hamster.StateTimer <= 0)
            {
                if (Rnd.NextDouble() < 0.2)
                {
                    hamster.CurrentState = Hamster.State.Idle;
                    hamster.StateTimer = Rnd.Next(30, 100);
                    hamster.CurrentFrame = 0;
                }
                else
                {
                    hamster.CurrentState = Hamster.State.Running;
                    hamster.StateTimer = Rnd.Next(100, 300);
                    if (Rnd.NextDouble() < 0.1)
                        hamster.CurrentDirection = hamster.CurrentDirection == Hamster.Direction.Left
                            ? Hamster.Direction.Right : Hamster.Direction.Left;
                    SetRandomVelocity(hamster);
                }
            }

            if (hamster.CurrentState == Hamster.State.Running)
            {
                double newX = hamster.Position.X + hamster.Velocity.X;
                double newY = hamster.Position.Y;

                var testX = new Rect(newX, newY, hamster.ImageWidth, hamster.ImageHeight);
                if (!IsInsideAnyScreen(testX, screens))
                {
                    newX = hamster.Position.X;
                    hamster.Velocity = new Vector(-hamster.Velocity.X, Math.Abs(hamster.Velocity.Y) * (Rnd.Next(2) == 0 ? 1 : -1));
                    hamster.CurrentDirection = hamster.Velocity.X < 0 ? Hamster.Direction.Left : Hamster.Direction.Right;
                }

                newY = hamster.Position.Y + hamster.Velocity.Y;
                var testY = new Rect(newX, newY, hamster.ImageWidth, hamster.ImageHeight);
                if (!IsInsideAnyScreen(testY, screens))
                {
                    newY = hamster.Position.Y;
                    hamster.Velocity = new Vector(hamster.Velocity.X, -hamster.Velocity.Y);
                }

                hamster.Position = new System.Windows.Point(newX, newY);

                hamster.FrameCounter++;
                if (hamster.FrameCounter >= hamster.FrameDelay)
                {
                    hamster.FrameCounter = 0;
                    hamster.CurrentFrame = (hamster.CurrentFrame + 1) % Hamster.MaxFrames;
                }
            }
        }

        private static void SetRandomVelocity(Hamster hamster)
        {
            double speed = Rnd.NextDouble() * 3.0 + 1.0;
            double angle = Rnd.NextDouble() * Math.PI * 2;
            hamster.Velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
            hamster.CurrentDirection = hamster.Velocity.X < 0 ? Hamster.Direction.Left : Hamster.Direction.Right;
        }

        private static bool IsInsideAnyScreen(Rect rect, List<Rect> screens)
        {
            foreach (var s in screens)
            {
                bool xIn = rect.Left >= s.Left - 1 && rect.Right <= s.Right + 1;
                bool yIn = rect.Top >= s.Top - 1 && rect.Bottom <= s.Bottom + 1;
                if (xIn && yIn) return true;
            }
            foreach (var s in screens)
            {
                var pts = new[]
                {
                    new System.Windows.Point(rect.Left, rect.Top),    new System.Windows.Point(rect.Right, rect.Top),
                    new System.Windows.Point(rect.Left, rect.Bottom), new System.Windows.Point(rect.Right, rect.Bottom)
                };
                foreach (var p in pts)
                    if (p.X >= s.Left - 1 && p.X <= s.Right + 1 && p.Y >= s.Top - 1 && p.Y <= s.Bottom + 1)
                        return true;
            }
            return false;
        }
    }
}

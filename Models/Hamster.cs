using System;
using System.Collections.Generic;
using System.Windows;
using HamsterDesktopRunners.Behaviors;

namespace HamsterDesktopRunners.Models
{
    public class Hamster
    {
        public enum Direction { Left, Right }
        public enum State { Running, Idle, Eating }

        public System.Windows.Point Position { get; set; }
        public System.Windows.Vector Velocity { get; set; }

        public Direction CurrentDirection { get; set; }
        public State CurrentState { get; set; }
        public HamsterType Type { get; }

        public int ImageWidth { get; } = 48;
        public int ImageHeight { get; } = 48;

        public int CurrentFrame { get; set; } = 0;
        public int FrameCounter { get; set; } = 0;
        public const int MaxFrames = 8;
        public int FrameDelay { get; } = 5;

        public int StateTimer { get; set; } = 0;
        public int EatTimer { get; set; } = 0;

        private readonly List<IHamsterBehavior> _behaviors;
        private static readonly Random Rnd = new Random();

        public Hamster(double startX, double startY, HamsterType type, List<IHamsterBehavior> behaviors)
        {
            Position = new System.Windows.Point(startX, startY);
            Type = type;
            _behaviors = behaviors;
            CurrentDirection = Rnd.Next(2) == 0 ? Direction.Left : Direction.Right;
            CurrentState = State.Running;
            StateTimer = Rnd.Next(60, 200);

            double speed = Rnd.NextDouble() * 3.0 + 1.0;
            double angle = Rnd.NextDouble() * Math.PI * 2;
            Velocity = new System.Windows.Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
            CurrentDirection = Velocity.X < 0 ? Direction.Left : Direction.Right;
        }

        public void Update(List<Rect> screens)
        {
            foreach (var behavior in _behaviors)
                behavior.Update(this, screens);
        }
    }
}

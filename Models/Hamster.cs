using System;
using System.Windows;

namespace HamsterDesktopRunners.Models
{
    public class Hamster
    {
        public enum Direction { Left, Right }
        public enum State { Running, Idle }

        public System.Windows.Point Position { get; set; }
        public System.Windows.Vector Velocity { get; set; }
        public Direction CurrentDirection { get; set; }
        public State CurrentState { get; set; }
        public HamsterType Type { get; }
        
        // 描画関連
        public int ImageWidth { get; } = 48;
        public int ImageHeight { get; } = 48;
        
        // アニメーション用フレーム
        public int CurrentFrame { get; set; } = 0;
        private int _frameCounter = 0;
        public const int MaxFrames = 8;
        public int FrameDelay { get; } = 5; // 何Tickごとにフレーム更新するか
        
        private static readonly Random Rnd = new Random();
        private int _stateTimer = 0;

        public Hamster(double startX, double startY, HamsterType type)
        {
            Position = new System.Windows.Point(startX, startY);
            Type = type;
            CurrentDirection = Rnd.Next(2) == 0 ? Direction.Left : Direction.Right;
            CurrentState = State.Running;
            SetRandomVelocity();
        }

        public void Update(System.Collections.Generic.List<Rect> screenBoundsList)
        {
            if (screenBoundsList == null || screenBoundsList.Count == 0) return;

            _stateTimer--;
            if (_stateTimer <= 0)
            {
                // 状態遷移
                if (Rnd.NextDouble() < 0.2)
                {
                    CurrentState = State.Idle;
                    _stateTimer = Rnd.Next(30, 100); // 0.5秒〜1.5秒程度待機
                    CurrentFrame = 0; // アイドル時はフレームを0にする（初期状態等）
                }
                else
                {
                    CurrentState = State.Running;
                    _stateTimer = Rnd.Next(100, 300); // 数秒走る
                    CurrentDirection = Rnd.NextDouble() < 0.1 ? 
                        (CurrentDirection == Direction.Left ? Direction.Right : Direction.Left) : 
                        CurrentDirection;
                    SetRandomVelocity();
                }
            }

            if (CurrentState == State.Running)
            {
                double newX = Position.X + Velocity.X;
                double newY = Position.Y;

                Rect testX = new Rect(newX, newY, ImageWidth, ImageHeight);
                if (!IsInsideAnyScreen(testX, screenBoundsList))
                {
                    newX = Position.X;
                    Velocity = new System.Windows.Vector(-Velocity.X, Math.Abs(Velocity.Y) * (Rnd.Next(2) == 0 ? 1 : -1));
                    CurrentDirection = Velocity.X < 0 ? Direction.Left : Direction.Right;
                }

                newY = Position.Y + Velocity.Y;
                Rect testY = new Rect(newX, newY, ImageWidth, ImageHeight);
                if (!IsInsideAnyScreen(testY, screenBoundsList))
                {
                    newY = Position.Y;
                    Velocity = new System.Windows.Vector(Velocity.X, -Velocity.Y);
                }

                Position = new System.Windows.Point(newX, newY);
                
                // アニメーション更新
                _frameCounter++;
                if (_frameCounter >= FrameDelay)
                {
                    _frameCounter = 0;
                    CurrentFrame = (CurrentFrame + 1) % MaxFrames;
                }
            }
        }

        private void SetRandomVelocity()
        {
            double speed = Rnd.NextDouble() * 3.0 + 1.0; // 1〜4px/frame
            double angle = Rnd.NextDouble() * Math.PI * 2; // 0〜2PI(全方位)
            Velocity = new System.Windows.Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed);
            CurrentDirection = Velocity.X < 0 ? Direction.Left : Direction.Right;
        }

        private bool IsInsideAnyScreen(Rect rect, System.Collections.Generic.List<Rect> screens)
        {
            return IsPointInScreens(new System.Windows.Point(rect.Left, rect.Top), screens) &&
                   IsPointInScreens(new System.Windows.Point(rect.Right, rect.Top), screens) &&
                   IsPointInScreens(new System.Windows.Point(rect.Left, rect.Bottom), screens) &&
                   IsPointInScreens(new System.Windows.Point(rect.Right, rect.Bottom), screens);
        }

        private bool IsPointInScreens(System.Windows.Point p, System.Collections.Generic.List<Rect> screens)
        {
            foreach (var s in screens)
            {
                if (p.X >= s.Left - 1 && p.X <= s.Right + 1 && p.Y >= s.Top - 1 && p.Y <= s.Bottom + 1)
                    return true;
            }
            return false;
        }
    }
}

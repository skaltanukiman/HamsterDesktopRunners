using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HamsterDesktopRunners.Models;
using HamsterDesktopRunners.Services;

namespace HamsterDesktopRunners.Views
{
    public partial class HamsterDesktopWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private HamsterManager _manager;
        private BitmapImage _spriteSheetGolden;
        private BitmapImage _spriteSheetDjungarian;
        private Rect _windowBounds;

        public HamsterDesktopWindow(HamsterManager manager, Rect windowBounds)
        {
            InitializeComponent();
            _manager = manager;
            _windowBounds = windowBounds;
            
            // 指定されたRectに合わせる
            this.Left = windowBounds.Left;
            this.Top = windowBounds.Top;
            this.Width = windowBounds.Width;
            this.Height = windowBounds.Height;

            _manager.HamstersUpdated += Manager_HamstersUpdated;

            // スプライトシートのロード(EmbeddedResource経由)
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            // ゴールデン
            using (var stream = assembly.GetManifestResourceStream("HamsterDesktopRunners.Assets.hamster_sprite_sheet.png"))
            {
                if (stream != null)
                {
                    _spriteSheetGolden = new BitmapImage();
                    _spriteSheetGolden.BeginInit();
                    _spriteSheetGolden.StreamSource = stream;
                    _spriteSheetGolden.CacheOption = BitmapCacheOption.OnLoad;
                    _spriteSheetGolden.EndInit();
                }
                else
                {
                    System.Windows.MessageBox.Show("ゴールデンハムスターのリソース取得に失敗しました", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    _spriteSheetGolden = new BitmapImage(); // dummy fallback
                }
            }

            // ジャンガリアン
            using (var stream = assembly.GetManifestResourceStream("HamsterDesktopRunners.Assets.hamster_djungarian.png"))
            {
                if (stream != null)
                {
                    _spriteSheetDjungarian = new BitmapImage();
                    _spriteSheetDjungarian.BeginInit();
                    _spriteSheetDjungarian.StreamSource = stream;
                    _spriteSheetDjungarian.CacheOption = BitmapCacheOption.OnLoad;
                    _spriteSheetDjungarian.EndInit();
                }
                else
                {
                    System.Windows.MessageBox.Show("ジャンガリアンハムスターのリソース取得に失敗しました", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    _spriteSheetDjungarian = new BitmapImage(); // dummy fallback
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // クリックスルーの設定
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        private void Manager_HamstersUpdated(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainCanvas.Children.Clear();

                foreach (var hamster in _manager.Hamsters)
                {
                    // ウィンドウ内の相対座標に変換
                    double x = hamster.Position.X - _windowBounds.Left;
                    double y = hamster.Position.Y - _windowBounds.Top;

                    // カリング（画面外は描画しない）
                    if (x + hamster.ImageWidth < 0 || x > _windowBounds.Width ||
                        y + hamster.ImageHeight < 0 || y > _windowBounds.Height)
                    {
                        continue; // returnではなくcontinueで次のハムスターへ
                    }

                    var img = new System.Windows.Controls.Image();
                    img.Width = hamster.ImageWidth;
                    img.Height = hamster.ImageHeight;

                    img.Source = hamster.Type == HamsterType.Djungarian
                        ? _spriteSheetDjungarian
                        : _spriteSheetGolden;

                    // 左右反転
                    if (hamster.CurrentDirection == Hamster.Direction.Left)
                    {
                        img.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                        img.RenderTransform = new ScaleTransform(-1, 1);
                    }

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    // Bobbingアニメーション
                    double bobbingOffset = 0;
                    if (hamster.CurrentState == Hamster.State.Running)
                        bobbingOffset = (hamster.CurrentFrame % 2 == 0) ? -2.0 : 0.0;

                    Canvas.SetLeft(img, x);
                    Canvas.SetTop(img, y + bobbingOffset);
                    MainCanvas.Children.Add(img);

                    // Eating状態: 🌾絵文字をオーバーレイ表示
                    if (hamster.CurrentState == Hamster.State.Eating)
                    {
                        var eatLabel = new System.Windows.Controls.TextBlock
                        {
                            Text = "🌾",
                            FontSize = 20,
                            IsHitTestVisible = false
                        };
                        Canvas.SetLeft(eatLabel, x + hamster.ImageWidth - 4);
                        Canvas.SetTop(eatLabel, y - 22);
                        MainCanvas.Children.Add(eatLabel);
                    }
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _manager.HamstersUpdated -= Manager_HamstersUpdated;
            base.OnClosed(e);
        }
    }
}

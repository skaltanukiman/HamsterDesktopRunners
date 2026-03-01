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
        private BitmapImage _seedImage;
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
            } // <- Djuingarianのusingブロックの閉じカッコ
            
            // 種の画像
            using (var stream = assembly.GetManifestResourceStream("HamsterDesktopRunners.Assets.sunflower_seed.png"))
            {
                if (stream != null)
                {
                    _seedImage = new BitmapImage();
                    _seedImage.BeginInit();
                    _seedImage.StreamSource = stream;
                    _seedImage.CacheOption = BitmapCacheOption.OnLoad;
                    _seedImage.EndInit();
                }
                else
                {
                    // フォールバック
                    _seedImage = new BitmapImage();
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

                    var transformGroup = new TransformGroup();

                    // Eating中はハムスター本体を潰したり伸ばしたりしてモグモグ表現 (Squash & Stretch)
                    if (hamster.CurrentState == Hamster.State.Eating)
                    {
                        double scaleX = hamster.CurrentFrame == 1 ? 1.03 : 0.97;
                        double scaleY = hamster.CurrentFrame == 1 ? 0.97 : 1.03;
                        transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
                    }

                    // 左右反転
                    if (hamster.CurrentDirection == Hamster.Direction.Left)
                    {
                        transformGroup.Children.Add(new ScaleTransform(-1, 1));
                    }

                    img.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    img.RenderTransform = transformGroup;

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    // Bobbingアニメーション
                    double bobbingOffset = 0;
                    if (hamster.CurrentState == Hamster.State.Running)
                        bobbingOffset = (hamster.CurrentFrame % 2 == 0) ? -2.0 : 0.0;

                    Canvas.SetLeft(img, x);
                    Canvas.SetTop(img, y + bobbingOffset);
                    MainCanvas.Children.Add(img);

                    // Eating状態: ひまわりの種を描画し、咀嚼に合わせて上下させる
                    if (hamster.CurrentState == Hamster.State.Eating)
                    {
                        var seedImg = new System.Windows.Controls.Image();
                        seedImg.Width = 16;
                        seedImg.Height = 16;
                        seedImg.Source = _seedImage;
                        RenderOptions.SetBitmapScalingMode(seedImg, BitmapScalingMode.NearestNeighbor);

                        // 食事の進行度に合わせて種を徐々に小さくする（食べ終わる表現）
                        double progress = Math.Max(0.0, (double)hamster.EatTimer / 120.0);
                        double seedScale = Math.Max(0.2, progress);
                        seedImg.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                        seedImg.RenderTransform = new ScaleTransform(seedScale, seedScale);

                        // 咀嚼アニメ（フレーム0と1で交互に上下する）に連動
                        double chewOffset = hamster.CurrentFrame == 1 ? 2.0 : -1.0;
                        
                        // 画像の向きに合わせて種の位置を口元寄りへ調整 (48x48の画像の中心近く)
                        if (hamster.CurrentDirection == Hamster.Direction.Left)
                        {
                            Canvas.SetLeft(seedImg, x + 8);
                        }
                        else
                        {
                            Canvas.SetLeft(seedImg, x + hamster.ImageWidth - 24);
                        }
                        
                        Canvas.SetTop(seedImg, y + hamster.ImageHeight - 24 + chewOffset);
                        MainCanvas.Children.Add(seedImg);
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

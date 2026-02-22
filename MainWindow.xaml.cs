using System;
using System.Drawing;
using System.Windows;
using HamsterDesktopRunners.Models;
using HamsterDesktopRunners.Services;
using HamsterDesktopRunners.Views;

namespace HamsterDesktopRunners;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private bool _isExit;
    private HamsterManager? _manager;
    private List<HamsterDesktopWindow> _windows = new List<HamsterDesktopWindow>();

    public MainWindow()
    {
        InitializeComponent();
        SetupNotifyIcon();
        LoadScreens();
    }

    public class ScreenItem
    {
        public string DisplayName { get; set; } = "";
        public Rect Bounds { get; set; }
        public override string ToString() => DisplayName;
    }

    private void LoadScreens()
    {
        CmbScreens.Items.Clear();
        CmbScreens.Items.Add(new ScreenItem() 
        { 
            DisplayName = "すべての画面 (Virtual Screen)",
            Bounds = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight)
        });

        int i = 1;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            CmbScreens.Items.Add(new ScreenItem()
            {
                DisplayName = $"ディスプレイ {i} ({screen.Bounds.Width}x{screen.Bounds.Height})",
                Bounds = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height)
            });
            i++;
        }
        CmbScreens.SelectedIndex = 0;
    }

    private void SetupNotifyIcon()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        _notifyIcon.Text = "HamsterDesktopRunners";
        _notifyIcon.Visible = true;
        
        // アイコン設定 (デフォルトアイコンを利用)
        _notifyIcon.Icon = SystemIcons.Application;
        
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        var showItem = new System.Windows.Forms.ToolStripMenuItem("表示");
        showItem.Click += (s, e) => ShowWindow();
        var exitItem = new System.Windows.Forms.ToolStripMenuItem("終了");
        exitItem.Click += (s, e) => ExitApplication();
        
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(exitItem);
        
        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExit)
        {
            e.Cancel = true;
            this.Hide();
        }
    }

    private void ExitApplication()
    {
        _isExit = true;
        _manager?.Stop();
        foreach (var w in _windows) w.Close();
        _notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        BtnStart.IsEnabled = false;
        BtnStop.IsEnabled = true;
        BtnAdd.IsEnabled = true;
        BtnClear.IsEnabled = true;
        CmbScreens.IsEnabled = false;
        
        var selectedScreen = CmbScreens.SelectedItem as ScreenItem;
        List<Rect> targetScreens = new List<Rect>();
        if (selectedScreen?.DisplayName.StartsWith("すべての画面") == true)
        {
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                targetScreens.Add(new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height));
            }
        }
        else if (selectedScreen != null)
        {
            targetScreens.Add(selectedScreen.Bounds);
        }
        else
        {
            targetScreens.Add(new Rect(0,0,1920,1080)); // fallback
        }

        if (_manager == null)
        {
            _manager = new HamsterManager(targetScreens);
            _manager.AddHamster(); // 初期で1匹追加
        }
        else
        {
            // 動的に変更された場合に対応
            _manager.ScreenBoundsList = targetScreens;
        }
        
        // 既存のウィンドウを閉じる
        foreach (var w in _windows) w.Close();
        _windows.Clear();

        // 対象ディスプレイの指定に関わらず、描画領域自体はすべての画面を覆う1枚のレイヤーにする
        Rect drawArea = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
        var desktopWindow = new HamsterDesktopWindow(_manager, drawArea);
        desktopWindow.Show();
        _windows.Add(desktopWindow);
        
        _manager.Start();
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        BtnStart.IsEnabled = true;
        BtnStop.IsEnabled = false;
        BtnAdd.IsEnabled = false;
        BtnClear.IsEnabled = false;
        CmbScreens.IsEnabled = true;
        
        _manager?.Stop();
        
        foreach (var w in _windows) w.Close();
        _windows.Clear();
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        _manager?.AddHamster();
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        _manager?.ClearHamsters();
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }
}
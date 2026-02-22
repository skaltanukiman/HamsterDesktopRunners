using System.Configuration;
using System.Data;
using System.Windows;

namespace HamsterDesktopRunners;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show($"アプリケーションエラーが発生しました:\n{e.Exception.Message}\n\nスタックトレース:\n{e.Exception.StackTrace}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is System.Exception ex)
        {
            System.Windows.MessageBox.Show($"致命的なエラーが発生しました:\n{ex.Message}\n\nスタックトレース:\n{ex.StackTrace}", "致命的なエラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}


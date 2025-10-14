using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using RouteNav.Avalonia;
using RouteNav.Avalonia.Stacks;

namespace ClatterCommunicator;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen splashScreen)
    {
        await Task.Delay(5000);
        desktop.MainWindow = new MainWindow();
        desktop.MainWindow.Show();
        splashScreen.Close();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splashscreen = new SplashScreen();
            desktop.MainWindow = splashscreen;
            ShowMainWindow(desktop, splashscreen);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
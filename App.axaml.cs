using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactorControl.ViewModels;
using ReactorControl.Views;
using ReactorControl.Models;
using Avalonia.Controls;

namespace ReactorControl;

public partial class App : Application
{
    public Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public Controller[] Controllers { get; set; } = System.Array.Empty<Controller>();

    public override void OnFrameworkInitializationCompleted()
    {
        LoadPersistent();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(Controllers),
            };
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    protected void LoadPersistent()
    {

    }
}
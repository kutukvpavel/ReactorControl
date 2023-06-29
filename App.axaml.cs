using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactorControl.ViewModels;
using ReactorControl.Views;
using ReactorControl.Models;
using Avalonia.Controls;
using LLibrary;
using System;
using System.Collections.ObjectModel;

namespace ReactorControl;

public partial class App : Application
{
    public Window? MainWindow { get; private set; }
    public L Logger { get; } = new L();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public ObservableCollection<Controller> Controllers { get; set; } = new ObservableCollection<Controller>();

    public override void OnFrameworkInitializationCompleted()
    {
        LoadPersistent();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new MainWindowViewModel(Controllers);
            vm.LoadRequest += Vm_LoadRequest;
            vm.SaveRequest += Vm_SaveRequest;
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Vm_SaveRequest(object? sender, System.EventArgs e)
    {
        SavePersistent();
    }

    private void Vm_LoadRequest(object? sender, System.EventArgs e)
    {
        LoadPersistent();
    }

    protected void LoadPersistent()
    {
        try
        {

        }
        catch (Exception)
        {

        }
    }
    protected void SavePersistent()
    {
        try
        {

        }
        catch (Exception)
        {

        }
    }
}
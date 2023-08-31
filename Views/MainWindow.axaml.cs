using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using ReactorControl.Providers;
using ReactorControl.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ReactorControl.Views;

public partial class MainWindow : Window
{
    protected const string Website = "https://github.com/kutukvpavel/ReactorControl";

    public MainWindow()
    {
        InitializeComponent();
        btnAbout.Click += BtnAbout_Click;
        btnProjectRepo.Click += BtnProjectRepo_Click;
        btnSettings.Click += BtnSettings_Click;
        btnConnectAll.Click += BtnConnectAll_Click;
        btnDisconnectAll.Click += BtnDisconnectAll_Click;
        btnRescanPorts.Click += BtnRescanPorts_Click;
        btnCreateExampleScript.Click += BtnCreateExampleScript_Click;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        ViewModel.SettingsContext.MainWindowWidth = Width;
        ViewModel.SettingsContext.MainWindowHeight = Height;
        await ViewModel.SaveSettings();
    }

    private void BtnRescanPorts_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in ViewModel.Controllers)
        {
            item.UpdatePort();
        }
    }

    private async void BtnDisconnectAll_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in ViewModel.Controllers)
        {
            if (item.CanDisconnect) await item.Disconnect();
        }
    }

    private async void BtnConnectAll_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in ViewModel.Controllers)
        {
            if (item.CanConnect) await item.Connect();
        }
    }

    private async void BtnSettings_Click(object? sender, RoutedEventArgs e)
    {
        SettingsDialog dialog = new()
        {
            DataContext = ViewModel.SettingsContext
        };
        if (await dialog.ShowDialog<bool>(this))
        {
            await ViewModel.SaveSettings();
        }
        await ViewModel.LoadSettings();
    }

    private async void BtnProjectRepo_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Website) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            var m = MessageBoxManager.GetMessageBoxStandard("ReactorControl",
                @$"Can't open web link: {Website}, details:
{ex.Message}");
            await m.ShowAsPopupAsync(this);
        }
    }

    private async void BtnAbout_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new AboutBox();
        await dialog.ShowDialog(this);
    }

    protected MainWindowViewModel ViewModel
    {
        get
        {
            if (DataContext == null) throw new NullReferenceException();
            return (MainWindowViewModel)DataContext;
        }
    }

    private async void AddRemove_Click(object? sender, RoutedEventArgs e)
    {
        var vm = new AddRemoveDevicesViewModel(ViewModel.ModelInstances);
        vm.ListChanged += Vm_ListChanged;
        var dialog = new AddRemoveDevices()
        {
            DataContext = vm
        };
        if (await dialog.ShowDialog<bool>(this))
        {
            await ViewModel.Save();
        }
        await ViewModel.Load();
        vm.ListChanged -= Vm_ListChanged;
    }

    private async void Vm_ListChanged(object? sender, EventArgs e)
    {
        await ViewModel.Trigger();
    }

    private async void BtnCreateExampleScript_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ScriptProvider.WriteExampleScript(Path.Combine(Environment.CurrentDirectory, "example_script.yaml"));
            var m = MessageBoxManager.GetMessageBoxStandard("Example Script", "Exmaple written OK. Check working directory.");
            await m.ShowAsPopupAsync(this);
        }
        catch (Exception ex)
        {
            var m = MessageBoxManager.GetMessageBoxStandard("Example Script", 
                $"Failed to write example script, details below.{Environment.NewLine}{ex}");
            await m.ShowAsPopupAsync(this);
        }
    }
}
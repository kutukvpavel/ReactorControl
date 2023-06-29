using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using ReactorControl.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
    }

    private void BtnDisconnectAll_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in ViewModel.Controllers)
        {
            if (item.CanDisconnect) item.Disconnect();
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
            var m = MessageBoxManager.GetMessageBoxStandardWindow("ReactorControl",
                @$"Can't open web link: {Website}, details:
{ex.Message}");
            await m.ShowDialog(this);
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
}
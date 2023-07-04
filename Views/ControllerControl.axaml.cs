using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactorControl.ViewModels;
using System;

namespace ReactorControl.Views;

public partial class ControllerControl : UserControl
{
    RegisterView? RegisterViewWindow;

    public ControllerControl()
    {
        InitializeComponent();
        btnConnect.Click += BtnConnect_Click;
        btnDisconnect.Click += BtnDisconnect_Click;
        expLeft.Expanding += ExpLeft_Expanding;
        chkPoll.Click += ChkPoll_Click;
        btnUpdateAll.Click += BtnUpdateAll_Click;
    }

    protected ControllerControlViewModel? ViewModel => DataContext as ControllerControlViewModel;

    private async void BtnUpdateAll_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SetStatus("Updating data...");
        await ViewModel?.Instance.ReadAll();
        ViewModel?.SetStatus("Update OK.");
    }
    private void ChkPoll_Click(object? sender, RoutedEventArgs e)
    {
        bool b = chkPoll.IsChecked ?? false;
        ViewModel?.Instance.SetAutoPoll(b);
        ViewModel?.SetStatus($"Polling is {(b ? "ON" : "OFF")}.");
    }
    private void BtnDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Disconnect();
    }
    private void ExpLeft_Expanding(object? sender, CancelRoutedEventArgs e)
    {
        ViewModel?.UpdatePort();
    }

    private void BtnConnect_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Connect();
    }

    public async void RegisterView_Click(object? sender, RoutedEventArgs e)
    {
        if (sender == null) return;
        if (DataContext is not ControllerControlViewModel vm) return;
        if (RegisterViewWindow != null)
        {
            RegisterViewWindow.Focus();
            return;
        }

        await vm.Instance.ReadAll();
        RegisterViewWindow = new();
        RegisterViewWindow.DataContext = new RegisterViewViewModel(vm, RegisterViewWindow);
        RegisterViewWindow.Closed += Vw_Closed;
        await RegisterViewWindow.ShowDialog((App.Current as App).MainWindow);
    }

    private void Vw_Closed(object? sender, EventArgs e)
    {
        if (RegisterViewWindow == null) throw new NullReferenceException();
        RegisterViewWindow.Closed -= Vw_Closed;
        RegisterViewWindow = null;
    }
}
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
        Loaded += ControllerControl_Loaded;
        btnConnect.Click += BtnConnect_Click;
        btnDisconnect.Click += BtnDisconnect_Click;
    }

    protected ControllerControlViewModel? ViewModel => DataContext as ControllerControlViewModel;

    private void BtnDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Disconnect();
    }

    private void BtnConnect_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Connect();
    }

    private void ControllerControl_Loaded(object? sender, RoutedEventArgs e)
    {
        ViewModel?.SetStatus("Not connnected.");
    }

    public void RegisterView_Click(object? sender, RoutedEventArgs e)
    {
        if (sender == null) return;
        if (DataContext is not ControllerControlViewModel vm) return;
        if (RegisterViewWindow != null)
        {
            RegisterViewWindow.Focus();
            return;
        }

        RegisterViewWindow = new() { DataContext = vm };
        RegisterViewWindow.Closed += Vw_Closed;
        RegisterViewWindow.Show();
    }

    private void Vw_Closed(object? sender, EventArgs e)
    {
        if (RegisterViewWindow == null) throw new NullReferenceException();
        RegisterViewWindow.Closed -= Vw_Closed;
        RegisterViewWindow = null;
    }
}
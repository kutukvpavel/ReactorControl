using Avalonia;
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

    private void Vw_Closed(object? sender, System.EventArgs e)
    {
        if (RegisterViewWindow == null) throw new NullReferenceException();
        RegisterViewWindow.Closed -= Vw_Closed;
        RegisterViewWindow = null;
    }
}
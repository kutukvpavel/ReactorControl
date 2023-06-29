using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactorControl.ViewModels;
using System;

namespace ReactorControl.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        btnAbout.Click += BtnAbout_Click;
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
        else
        {
            await ViewModel.Load();
        }
        vm.ListChanged -= Vm_ListChanged;
    }

    private async void Vm_ListChanged(object? sender, EventArgs e)
    {
        await ViewModel.Trigger();
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ReactorControl.Views
{
    public partial class InterfaceState : UserControl
    {
        public InterfaceState()
        {
            InitializeComponent();
        }

        public async void Remote_Click(object? sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).RemoteControl((sender as CheckBox)?.IsChecked ?? false);
        }
        public async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).SaveNVS();
        }
        public async void Reload_Click(object? sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).ReloadParams();
        }
        public async void Reboot_Click(object? sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).Reset();
        }
    }
}

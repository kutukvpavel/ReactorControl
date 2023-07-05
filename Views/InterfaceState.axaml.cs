using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ReactorControl.Views
{
    public partial class InterfaceState : UserControl
    {
        public InterfaceState()
        {
            InitializeComponent();
            btnDFU.Click += BtnDFU_Click;
        }

        private async void BtnDFU_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).EnterDFU();
        }
        private async void Remote_Click(object? sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).RemoteControl((sender as CheckBox)?.IsChecked ?? false);
        }
        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).SaveNVS();
        }
        private async void Reload_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).ReloadParams();
        }
        private async void Reboot_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;
            await ((ViewModels.InterfaceStateViewModel)DataContext).Reset();
        }
    }
}

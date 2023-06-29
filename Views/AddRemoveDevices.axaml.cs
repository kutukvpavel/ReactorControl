using Avalonia.Controls;
using ReactorControl.ViewModels;

namespace ReactorControl.Views
{
    public partial class AddRemoveDevices : Window
    {
        protected bool Save = false;

        public AddRemoveDevices()
        {
            InitializeComponent();
            tabDevices.SelectionChanged += TabDevices_SelectionChanged;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;
        }

        private void BtnRemove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as AddRemoveDevicesViewModel)?.Remove(tabDevices.SelectedItem as DeviceEditViewModel);
        }

        private void BtnAdd_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as AddRemoveDevicesViewModel)?.Add();
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(Save);
        }

        private void BtnOK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Save = true;
            Close(Save);
        }

        private void TabDevices_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            (DataContext as AddRemoveDevicesViewModel)?.Trigger();
        }
    }
}

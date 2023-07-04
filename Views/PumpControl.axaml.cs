using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactorControl.ViewModels;
using System.Globalization;

namespace ReactorControl.Views
{
    public partial class PumpControl : UserControl
    {
        public PumpControl()
        {
            InitializeComponent();
            btnZero.Click += BtnZero_Click;
        }

        public async void BtnZero_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not PumpControlViewModel vm) return;

            await vm.SetVolumeRate(0);
        }

        public async void Commanded_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox txt) return;
            if (DataContext is not PumpControlViewModel vm) return;

            if (float.TryParse(txt.Text, NumberStyles.Float, CultureInfo.CurrentUICulture, out float v))
            {
                if (e.Key == Key.Enter) await vm.SetVolumeRate(v);
                txt.BorderBrush = Brushes.Green;
            }
            else
            {
                txt.BorderBrush = Brushes.Coral;
            }
        }
    }
}

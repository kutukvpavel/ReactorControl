using Avalonia.Controls;
using Avalonia.Input;
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
        }

        public async void Commanded_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox txt) return;
            if (DataContext is not PumpControlViewModel vm) return;

            if (float.TryParse(txt.Text, NumberStyles.Float, CultureInfo.CurrentUICulture, out float v))
            {
                await vm.SetVolumeRate(v);
                txt.BorderBrush = Brushes.Green;
            }
            else
            {
                txt.BorderBrush = Brushes.Coral;
            }
        }
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactorControl.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ReactorControl.Views
{
    public partial class PumpControl : UserControl
    {
        private async Task TrySetFloat(object? sender, KeyEventArgs e, Func<PumpControlViewModel, float, Task> a)
        {
            if (sender is not TextBox txt) return;
            if (DataContext is not PumpControlViewModel vm) return;

            if (float.TryParse(txt.Text, NumberStyles.Float, CultureInfo.CurrentUICulture, out float v))
            {
                if (e.Key == Key.Enter) await a(vm, v);
                txt.BorderBrush = Brushes.Green;
            }
            else
            {
                txt.BorderBrush = Brushes.Coral;
            }
        }

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
        public async void BtnGo_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not PumpControlViewModel vm) return;

            await vm.RunTimer();
        }
        public async void EnableTimer_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (DataContext is not PumpControlViewModel vm) return;

            await vm.EnableTimer(!(vm.TimerEnabled ?? true));
        }

        public async void Commanded_KeyDown(object? sender, KeyEventArgs e)
        {
            await TrySetFloat(sender, e, async (vm, v) => await vm.SetVolumeRate(v));
        }

        public async void CommandedTime_KeyDown(object? sender, KeyEventArgs e)
        {
            await TrySetFloat(sender, e, async (vm, v) => await vm.SetTimer(v));
        }

        public async void CommandedVolume_KeyDown(object? sender, KeyEventArgs e)
        {
            await TrySetFloat(sender, e, async (vm, v) =>
            {
                // t = V / Q
                v /= vm.CommandedSpeedRegister.TypedValue.Value;
                await vm.SetTimer(v);
            });
        }
    }
}

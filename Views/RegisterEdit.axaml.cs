using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactorControl.ViewModels;

namespace ReactorControl.Views;

public partial class RegisterEdit : UserControl
{
    public RegisterEdit()
    {
        InitializeComponent();
        mtbInput.AddHandler(KeyDownEvent, Textbox_KeyDown, RoutingStrategies.Tunnel);
    }

    private async void Textbox_KeyDown(object? sender, KeyEventArgs e)
    {
        var vm = DataContext as RegisterEditViewModel;
        var tb = sender as TextBox;
        if (tb?.Text == null || vm == null) return;
        if (tb.IsReadOnly) return;
        if (e.Key == Key.Delete || e.Key == Key.Back || e.Key == Key.Space) return;

        vm.TextboxValue = tb.Text.Trim();
        if (e.Key == Key.Enter)
        {
            await vm.Write();
        }
        else
        {
            vm.TrySet();
        }
    }

    public async void R_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;
        if (DataContext is not RegisterEditViewModel vm) return;

        await vm.Read();
    }
    public async void W_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;
        if (DataContext is not RegisterEditViewModel vm) return;
        if (mtbInput.Text == null) return;

        vm.TextboxValue = mtbInput.Text.Trim();
        await vm.Write();
    }
    public async void Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;
        if (DataContext is not RegisterEditViewModel vm) return;

        await vm.Edit(vm.Owner);
    }
}
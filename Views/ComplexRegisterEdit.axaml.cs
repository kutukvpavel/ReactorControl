using Avalonia.Controls;
using Avalonia.Interactivity;
using ModbusRegisterMap;

namespace ReactorControl.Views
{
    public partial class ComplexRegisterEdit : Window
    {
        public ComplexRegisterEdit()
        {
            InitializeComponent();
        }

        public bool Result { get; private set; } = false;

        public void OK_Click(object? sender, RoutedEventArgs e)
        {
            if (! ((DataContext as IRegister)?.IsReadOnly ?? true) ) Result = true;
            Close(Result);
        }
        public void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(Result);
        }
    }
}

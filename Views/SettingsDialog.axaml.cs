using Avalonia.Controls;

namespace ReactorControl.Views
{
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
            btnOK.Click += BtnOK_Click;
        }

        public bool DialogResult { get; private set; } = false;

        private void BtnOK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DialogResult = true;
            Close(DialogResult);
        }
    }
}

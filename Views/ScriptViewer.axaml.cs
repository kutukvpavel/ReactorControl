using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactorControl.ViewModels;

namespace ReactorControl.Views
{
    public partial class ScriptViewer : UserControl
    {
        public ScriptViewer()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ScriptViewerViewModel vm) return;

            try
            {
                vm.GetProvider()?.Start();
            }
            catch (InvalidOperationException)
            {
                
            }
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ScriptViewerViewModel vm) return;

            try
            {
                vm.GetProvider()?.Stop();
            }
            catch (InvalidOperationException)
            {
                
            }
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ScriptViewerViewModel vm) return;

            try
            {
                vm.GetProvider()?.Pause();
            }
            catch (InvalidOperationException)
            {
                
            }
        }
        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ScriptViewerViewModel vm) return;

            try
            {
                vm.GetProvider()?.Resume();
            }
            catch (InvalidOperationException)
            {
                
            }
        }
    }
}

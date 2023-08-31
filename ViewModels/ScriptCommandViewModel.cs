using System;
using System.Threading.Tasks;
using Avalonia.Media;
using ReactorControl.Providers;

namespace ReactorControl.ViewModels
{
    public class ScriptCommandViewModel : ViewModelBase
    {
        protected ScriptProvider.Command mCommand;

        public ScriptCommandViewModel(ScriptProvider.Command c)
        {
            mCommand = c;
            mCommand.ActivityChanged += mCommand_ActivityChanged;
        }

        public string Text
        {
            get {
                if (mCommand.Description != null) return mCommand.Description;
                if (mCommand.TotalTime != null) return $"{mCommand.TotalTime:F1} @ {mCommand.VolumeRate:F3}";
                if (mCommand.TotalVolume != null) return $"{mCommand.TotalVolume:F3} @ {mCommand.VolumeRate:F3}";
                return "N/A";
            }
        }
        public IBrush HighlightBrush => mCommand.IsActive ? Brushes.LightGreen : Brushes.LightBlue;

        private void mCommand_ActivityChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(HighlightBrush));
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;
using ReactorControl.Models;
using ReactorControl.Providers;

namespace ReactorControl.ViewModels
{
    public class ScriptViewerViewModel : ViewModelBase
    {
        protected ScriptProvider? mProvider;
        protected Controller mController;

        public ScriptViewerViewModel(Controller c)
        {
            mController = c;
            mController.PropertyChanged += Controller_PropertyChanged;
        }

        public string ScriptName => mProvider?.ScriptInstance?.Name ?? string.Empty;
        public ObservableCollection<ScriptThreadViewModel> Threads { get; } = new();
        private bool CanBase => mController.IsConnected && (mController.Mode == Constants.Modes.Auto) && !ValidationMode;
        public bool CanStart => CanBase && (mProvider?.State == ScriptProvider.ExecutionState.Stopped);
        public bool CanStop => CanBase && (mProvider != null);
        public bool CanPause => CanBase && (mProvider?.State == ScriptProvider.ExecutionState.Running);
        public bool CanResume => CanBase && (mProvider?.State == ScriptProvider.ExecutionState.Paused);
        public bool ValidationMode { get; private set; } = false;
        public string Status {
            get {
                if (mProvider == null) return "N/A";
                if (mProvider.IsAborted) return "Aborted!";
                if (mProvider.IsCompleted) return "Completed";
                return Enum.GetName(typeof(ScriptProvider.ExecutionState), mProvider.State) ?? "N/A";
            }
        }
        public IBrush StatusColor {
            get {
                if (mProvider == null) return Brushes.LightGray;
                if (mProvider.IsAborted) return Brushes.LightCoral;
                if (mProvider.IsCompleted) return Brushes.LightGreen;
                switch (mProvider.State)
                {
                    case ScriptProvider.ExecutionState.Running:
                        return Brushes.LightGreen;
                    case ScriptProvider.ExecutionState.Paused:
                        return Brushes.LightBlue;
                    case ScriptProvider.ExecutionState.Stopped:
                        return Brushes.LightSalmon;
                    default:
                        return Brushes.LightGray;
                }
            }
        }
        public bool ShowProgress => 
            (mProvider?.State ?? ScriptProvider.ExecutionState.Stopped) == ScriptProvider.ExecutionState.Running;
        public double Progress => mProvider?.Progress ?? 0;

        public void SetProvider(ScriptProvider? p, bool validationMode = false)
        {
            if (mProvider != null) mProvider.PropertyChanged -= Provider_PropertyChanged;
            mProvider = p;
            Threads.Clear();
            if (mProvider == null) return;
            mProvider.PropertyChanged += Provider_PropertyChanged;
            foreach (var t in mProvider.ScriptInstance.Threads)
            {
                Threads.Add(new ScriptThreadViewModel(t));
            }
            ValidationMode = validationMode;
            RaisePropertyChanged(nameof(ScriptName));
            RaisePropertyChanged(nameof(Threads));
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanResume));
            RaisePropertyChanged(nameof(ValidationMode));
        }
        public ScriptProvider? GetProvider()
        {
            return mProvider;
        }

        private void Controller_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Controller.IsConnected) && e.PropertyName != nameof(Controller.Mode)) return;
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanResume));
        }
        private void Provider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ScriptProvider.State)) return;
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanResume));
            RaisePropertyChanged(nameof(Status));
            RaisePropertyChanged(nameof(StatusColor));
            RaisePropertyChanged(nameof(ShowProgress));
            RaisePropertyChanged(nameof(Progress));
        }
    }
}
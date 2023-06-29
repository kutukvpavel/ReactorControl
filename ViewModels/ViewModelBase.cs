using Avalonia.Threading;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactorControl.ViewModels;

public class ViewModelBase : ReactiveObject, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LogEventArgs>? LogDataReceived;

    protected void RaisePropertyChanged([CallerMemberName] string? name = null)
    {
        Dispatcher.UIThread.Post(() => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))
        );
    }

    protected void Log(string msg, Exception? ex = null)
    {
        LogDataReceived?.Invoke(this, new LogEventArgs(ex, msg));
    }
    protected void Log(object? sender, LogEventArgs e)
    {
        LogDataReceived?.Invoke(sender, e);
    }
}

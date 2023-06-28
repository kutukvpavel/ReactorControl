using Avalonia.Threading;
using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactorControl.ViewModels;

public class ViewModelBase : ReactiveObject, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? name = null)
    {
        Dispatcher.UIThread.Post(() => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))
        );
    }
}

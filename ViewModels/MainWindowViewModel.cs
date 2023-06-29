using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public event EventHandler? SaveRequest;
    public event EventHandler? LoadRequest;

    public MainWindowViewModel(ObservableCollection<Controller> c)
    {
        ModelInstances = c;
        Controllers = new ObservableCollection<ControllerControlViewModel>();
        for (int i = 0; i < c.Count; i++)
        {
            Controllers[i] = new ControllerControlViewModel(c[i]);
        }
    }

    public ObservableCollection<Controller> ModelInstances { get; }
    public ObservableCollection<ControllerControlViewModel> Controllers { get; }

    public async Task Save()
    {
        await Task.Run(() => SaveRequest?.Invoke(this, new EventArgs()));
    }
    public async Task Load()
    {
        await Task.Run(() => LoadRequest?.Invoke(this, new EventArgs()));
    }
    public async Task Trigger()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Controllers.Clear();
            Controllers.AddRange(ModelInstances.Select(x => new ControllerControlViewModel(x)));
        });
    }
}

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
    public event EventHandler? SettingsSaveRequested;
    public event EventHandler? SettingsLoadRequested;

    public MainWindowViewModel(ObservableCollection<Controller> c, Settings settingsContext)
    {
        ModelInstances = c;
        Controllers = new ObservableCollection<ControllerControlViewModel>();
        Controllers.AddRange(ModelInstances.Select(x => new ControllerControlViewModel(x)));
        SettingsContext = settingsContext;
        foreach (var item in Controllers)
        {
            item.LogDataReceived += Item_LogDataReceived;
        }
    }

    public ObservableCollection<Controller> ModelInstances { get; }
    public ObservableCollection<ControllerControlViewModel> Controllers { get; }
    public Settings SettingsContext { get; set; }

    public async Task SaveSettings()
    {
        await Task.Run(() => { SettingsSaveRequested?.Invoke(this, new EventArgs()); });
    }
    public async Task LoadSettings()
    {
        await Task.Run(() => { SettingsLoadRequested?.Invoke(this, new EventArgs()); });
    }
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
        foreach (var item in Controllers)
        {
            item.LogDataReceived -= Item_LogDataReceived;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Controllers.Clear();
            Controllers.AddRange(ModelInstances.Select(x => new ControllerControlViewModel(x)));
        });
        foreach (var item in Controllers)
        {
            item.LogDataReceived += Item_LogDataReceived;
        }
    }

    private void Item_LogDataReceived(object? sender, LogEventArgs e)
    {
        Log(sender, e);
    }
}

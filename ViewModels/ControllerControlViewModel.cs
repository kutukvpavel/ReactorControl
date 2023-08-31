using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls;
using DynamicData;
using ModbusRegisterMap;
using ReactorControl.Models;
using ReactorControl.Providers;
using RJCP.IO.Ports;
using MsBox.Avalonia;

namespace ReactorControl.ViewModels;

public class ControllerControlViewModel : ViewModelBase
{
    public static bool AutoPollAfterConnection {get;set;} = false;

    public ControllerControlViewModel(Controller c, Settings context)
    {
        SettingsContext = context;
        Instance = c;
        InterfaceState = new InterfaceStateViewModel(c);
        Instance.PropertyChanged += Instance_PropertyChanged;
        Instance.LogEvent += Instance_LogEvent;
        Instance.UnexpectedDisconnect += Instance_UnexpectedDisconnect;
        Instance.PollCompleted += Instance_PollCompleted;
        if (Instance.Config.IpcSocketPort > 0)
        {
            OutProviders.Add(new SocketProvider(Name,
                new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, Instance.Config.IpcSocketPort)));
        }
        if (Instance.Config.IpcPipeName.Length > 0)
        {
            OutProviders.Add(new PipesProvider(Name));
        }
        if (SettingsContext.CsvFolder.Length > 0)
        {
            OutProviders.Add(new CsvProvider(SettingsContext.CsvFolder, Name));
        }
        ScriptViewerInstance = new ScriptViewerViewModel(Instance);
    }

    private async void Item_CommandReceived(object? sender, IpcEventArgs e)
    {
        if (e.TargetDeviceName != Name) return;
        if (e.RegisterName == null)
        {
            if (sender is IOutputProvider p)
            {
                p.SendTest();
            }
            return;
        }
        var reg = Instance.RegisterMap.Find(e.RegisterName);
        if (reg == null || e.ValueString == null) return;
        try
        {
            reg.Value.TrySet(e.ValueString);
            await Instance.WriteRegister(reg);
            await Instance.ReadRegister(reg);
            Log($"Applied IPC message: '{reg.Name}'@'{Name}' = '{e.ValueString}'");
        }
        catch (Exception ex)
        {
            Log($"failed to apply IPC message for '{reg.Name}'@'{Name}' (set to '{e.ValueString}')", ex);
        }
    }
    private void Instance_PollCompleted(object? sender, Controller.PollResult e)
    {
        foreach (var item in OutProviders)
        {
            foreach (var reg in e.UpdatedRegisters)
            {
                item.Send(reg);
            }
        }
    }
    private void Instance_UnexpectedDisconnect(object? sender, EventArgs e)
    {
        SetStatus("Unexpected disconnect!");
    }
    private void Instance_LogEvent(object? sender, LogEventArgs e)
    {
        Log(sender, e);
    }
    private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Instance.IsConnected))
        {
            RaisePropertyChanged(nameof(CanConnect));
            RaisePropertyChanged(nameof(CanDisconnect));
            RaisePropertyChanged(nameof(IsConnected));
            foreach (var item in OutProviders)
            {
                if (IsConnected) item.Connect();
                else item.Disconnect();
            }
        }
        if (e.PropertyName == nameof(Instance.IsPolling)) RaisePropertyChanged(nameof(IsPolling));
        if (e.PropertyName == nameof(Instance.Mode))
        {
            RaisePropertyChanged(nameof(ModeString));
            RaisePropertyChanged(nameof(ModeColor));
        }
        if (e.PropertyName == nameof(Instance.TotalPumps))
        {
            var p = new PumpControlViewModel[Instance.TotalPumps];
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = new PumpControlViewModel(Instance, i);
            }
            Dispatcher.UIThread.Post(() =>
            {
                Pumps.Clear();
                Pumps.AddRange(p);
            });
        }
        if (e.PropertyName == nameof(Instance.TotalProbes))
        {
            var p = Instance.Config.Probes.Select(x => new ProbeControlViewModel(Instance, x));
            Dispatcher.UIThread.Post(() =>
            {
                Probes.Clear();
                if (IsConnected) Probes.AddRange(p);
            });
        }
    }

    public Controller Instance { get; }
    public OrderedDictionary HoldingRegisters => Instance.RegisterMap.HoldingRegisters;
    public OrderedDictionary InputRegisters => Instance.RegisterMap.InputRegisters;
    public InterfaceStateViewModel InterfaceState { get; }
    public ObservableCollection<PumpControlViewModel> Pumps { get; } = new ObservableCollection<PumpControlViewModel>();
    public ObservableCollection<ProbeControlViewModel> Probes { get; } = new ObservableCollection<ProbeControlViewModel>();
    public bool CanConnect => !IsConnected && SerialPortStream.GetPortNames().Contains(Instance.Config.PortName);
    public bool CanDisconnect => Instance.IsConnected;
    public string Status { get; set; } = "Not connected.";
    public string PortNameString => $"Port: {PortName}";
    public string ModeString
    {
        get
        {
            return Instance.Mode switch
            {
                Constants.Modes.Init => "Init",
                Constants.Modes.Manual => "Manual",
                Constants.Modes.Auto => "Auto",
                Constants.Modes.Emergency => "EMERGENCY",
                Constants.Modes.LampTest => "Lamp Test",
                _ => "N/A",
            };
        }
    }
    public IBrush ModeColor => Instance.Mode switch
    {
        Constants.Modes.Init => Brushes.LightBlue,
        Constants.Modes.Manual => Brushes.Yellow,
        Constants.Modes.Auto => Brushes.LightGreen,
        Constants.Modes.Emergency => Brushes.LightCoral,
        Constants.Modes.LampTest => Brushes.Magenta,
        _ => Brushes.LightGray
    };
    public bool CanOpenScript => (ScriptInstance?.State ?? ScriptProvider.ExecutionState.Stopped)
        == ScriptProvider.ExecutionState.Stopped;
    public ScriptViewerViewModel ScriptViewerInstance { get; }

    public string Name
    {
        get => Instance.Config.Name;
        set
        {
            Instance.Config.Name = value;
        }
    }
    public bool IsConnected => Instance.IsConnected;
    public bool IsPolling => Instance.IsPolling;
    public string PortName
    {
        get => Instance.Config.PortName;
        set
        {
            Instance.Config.PortName = value;
        }
    }
    public Color TabColor => IsConnected ? Colors.LimeGreen : Colors.LightCoral;

    public Settings SettingsContext { get; private set; }
    public List<IOutputProvider> OutProviders { get; } = new List<IOutputProvider>();
    public List<IInputProvider> InProviders { get; } = new List<IInputProvider>();
    public ScriptProvider? ScriptInstance { get; private set; }

    public async Task Connect()
    {
        SetStatus("Connecting...");
        bool b = await Instance.Connect(PortName);
        SetStatus(b ? "Ready." : "Failed.");
        await Instance.ReadAll();
        await Instance.ReadAll();
        if (b && AutoPollAfterConnection) Instance.SetAutoPoll(true);
    }
    public void UpdatePort()
    {
        RaisePropertyChanged(nameof(CanConnect));
    }
    public async Task Disconnect()
    {
        SetStatus((await Instance.Disconnect()) ? "Disconnected OK." : "Diconnect failed.");
    }
    public void SetStatus(string s)
    {
        Status = s;
        RaisePropertyChanged(nameof(Status));
    }
    public void UpdateSettingsContext(Settings s)
    {
        SettingsContext = s;
        RaisePropertyChanged(nameof(SettingsContext));
    }
    public async Task OpenScriptFile()
    {
        if (!CanOpenScript) return;

        var dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter() { Name = "YAML files", Extensions = new List<string>() { "yaml" } });
        dialog.Title = "Select script file...";
        dialog.Directory = Environment.CurrentDirectory;
        var files = await dialog.ShowAsync((App.Current as App).MainWindow);
        if ((files?.Length ?? 0) == 0) return;
        
        try
        {
            var s = await ScriptProvider.ReadScript(files[0]);
            if (s.PumpsUsed.Any(x => x >= Instance.TotalPumps) && IsConnected)
                throw new InvalidOperationException("Some of the specified pumps don't exist in this device");
            s.TargetDeviceName = Instance.Config.Name;
            ScriptInstance = new ScriptProvider(s);
            ScriptInstance.CommandReceived += Item_CommandReceived;
            ScriptViewerInstance.SetProvider(ScriptInstance);
            RaisePropertyChanged(nameof(ScriptViewerInstance));
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Error", ex.ToString());
        }
    }
}
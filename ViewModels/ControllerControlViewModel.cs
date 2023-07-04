using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using ReactorControl.Models;
using RJCP.IO.Ports;

namespace ReactorControl.ViewModels;

public class ControllerControlViewModel : ViewModelBase
{
    public ControllerControlViewModel(Controller c)
    {
        Instance = c;
        InterfaceState = new InterfaceStateViewModel(c);
        Instance.PropertyChanged += Instance_PropertyChanged;
        Instance.LogEvent += Instance_LogEvent;
        Instance.UnexpectedDisconnect += Instance_UnexpectedDisconnect;
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
        RaisePropertyChanged(nameof(CanConnect));
        RaisePropertyChanged(nameof(CanDisconnect));
        RaisePropertyChanged(nameof(IsConnected));
        RaisePropertyChanged(nameof(IsPolling));

        if (e.PropertyName == nameof(Instance.TotalPumps)) return;
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

    public Controller Instance { get; }
    public OrderedDictionary HoldingRegisters => Instance.RegisterMap.HoldingRegisters;
    public OrderedDictionary InputRegisters => Instance.RegisterMap.InputRegisters;
    public InterfaceStateViewModel InterfaceState { get; }
    public ObservableCollection<PumpControlViewModel> Pumps { get; } = new ObservableCollection<PumpControlViewModel>();
    public bool CanConnect => !IsConnected && SerialPortStream.GetPortNames().Contains(Instance.Config.PortName);
    public bool CanDisconnect => Instance.IsConnected;
    public string Status { get; set; } = "Not connected.";
    public string PortNameString => $"Port: {PortName}";

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

    public async Task Connect()
    {
        SetStatus("Connecting...");
        bool success = await Instance.Connect(PortName);
        SetStatus(success ? "Connected OK." : "Connection failed.");
        await Instance.ReadAll();
    }
    public void UpdatePort()
    {
        RaisePropertyChanged(nameof(CanConnect));
    }
    public void Disconnect()
    {
        SetStatus(Instance.Disconnect() ? "Disconnected OK." : "Diconnect failed.");
    }
    public void SetStatus(string s)
    {
        Status = s;
        RaisePropertyChanged(nameof(Status));
    }
}
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using ReactorControl.Models;
using RJCP.IO.Ports;

namespace ReactorControl.ViewModels;

public class ControllerControlViewModel : ViewModelBase
{
    public ControllerControlViewModel(Controller c)
    {
        Instance = c;
        InterfaceState = new InterfaceStateViewModel(c);
        Pumps = new PumpControlViewModel[c.TotalPumps];
        for (int i = 0; i < Pumps.Length; i++)
        {
            Pumps[i] = new PumpControlViewModel(c, i);
        }
        Instance.PropertyChanged += Instance_PropertyChanged;
        Instance.LogEvent += Instance_LogEvent;
    }

    private void Instance_LogEvent(object? sender, LogEventArgs e)
    {
        Log(sender, e);
    }

    private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(CanConnect));
        RaisePropertyChanged(nameof(CanDisconnect));
    }

    public Controller Instance { get; }
    public OrderedDictionary HoldingRegisters => Instance.RegisterMap.HoldingRegisters;
    public OrderedDictionary InputRegisters => Instance.RegisterMap.InputRegisters;
    public InterfaceStateViewModel InterfaceState { get; }
    public PumpControlViewModel[] Pumps { get; }
    public bool CanConnect => !Instance.IsConnected && SerialPortStream.GetPortNames().Contains(Instance.Config.PortName);
    public bool CanDisconnect => Instance.IsConnected;
    public string Status { get; set; } = "Not connected.";

    public string Name
    {
        get => Instance.Config.Name;
        set
        {
            Instance.Config.Name = value;
        }
    }
    public bool IsConnected => Instance.IsConnected;
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
        SetStatus(await Instance.Connect(PortName) ? "Connected OK." : "Connection failed.");
    }
    public void Disconnect()
    {
        SetStatus(Instance.Disconnect() ? "Disconnected OK." : "Diconnect failed.");
    }

    public void Trigger()
    {
        Dispatcher.UIThread.Post(() => { RaisePropertyChanged(nameof(CanConnect)); });
    }
    public void SetStatus(string s)
    {
        Status = s;
        RaisePropertyChanged(nameof(Status));
    }
}
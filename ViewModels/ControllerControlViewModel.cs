using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia.Media;
using ReactorControl.Models;

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
    }

    public Controller Instance { get; }
    public OrderedDictionary HoldingRegisters => Instance.RegisterMap.HoldingRegisters;
    public OrderedDictionary InputRegisters => Instance.RegisterMap.InputRegisters;
    public InterfaceStateViewModel InterfaceState { get; }
    public PumpControlViewModel[] Pumps { get; }

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
        await Instance.Connect(PortName);
    }
}
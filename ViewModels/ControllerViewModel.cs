using System;
using System.Threading.Tasks;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class ContorllerViewModel : ViewModelBase
{
    public ContorllerViewModel(Controller c, ControllerConfig cfg)
    {
        Instance = c;
        Config = cfg;
    }

    public Controller Instance { get; }
    public ControllerConfig Config {get;}

    public string Name
    {
        get => Config.Name;
        set
        {
            Config.Name = value;
        }
    }
    public bool IsConnected => Instance.IsConnected;
    public string PortName
    {
        get => Config.PortName;
        set
        {
            Config.PortName = value;
        }
    }

    public async Task Connect()
    {
        await Instance.Connect(PortName);
    }
}
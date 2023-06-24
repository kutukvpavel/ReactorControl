using System;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Controller[] Instances;

    public MainWindowViewModel(Controller[] c, ControllerConfig[] cfg)
    {
        Instances = c;
        Controllers = new ContorllerViewModel[c.Length];
        for (int i = 0; i < c.Length; i++)
        {
            if (cfg[i].ConnectionType != ControllerConfig.ConnectionTypes.Serial)
            {
                Console.WriteLine($"Connection type unsupproted: {cfg[i].ConnectionType}");
                continue;
            }
            Controllers[i] = new ContorllerViewModel(c[i], cfg[i]);
        }
    }

    public ContorllerViewModel[] Controllers { get; }

    
}

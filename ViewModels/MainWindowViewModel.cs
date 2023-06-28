using System;
using ReactorControl.Models;

namespace ReactorControl.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    protected readonly Controller[] Instances;

    public MainWindowViewModel(Controller[] c)
    {
        Instances = c;
        Controllers = new ControllerControlViewModel[c.Length];
        for (int i = 0; i < c.Length; i++)
        {
            Controllers[i] = new ControllerControlViewModel(c[i]);
        }
    }

    public ControllerControlViewModel[] Controllers { get; }
}

using ReactorControl.Models;
using RJCP.IO.Ports;

namespace ReactorControl.ViewModels
{
    public class DeviceEditViewModel : ViewModelBase
    {
        public DeviceEditViewModel(Controller c)
        {
            Instance = c;
            Configuration = c.Config;
        }

        public Controller Instance { get; }
        public ControllerConfig Configuration { get; }
        public bool CanEdit => !Instance.IsConnected;
        public string Header => Configuration.Name;
        public string[] AvailablePorts => SerialPortStream.GetPortNames();
    }
}

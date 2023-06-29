using Avalonia.Threading;
using ReactorControl.Models;
using RJCP.IO.Ports;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ReactorControl.ViewModels
{
    public class AddRemoveDevicesViewModel : ViewModelBase
    {
        protected ObservableCollection<Controller> mList;

        public event EventHandler? ListChanged;

        public AddRemoveDevicesViewModel(ObservableCollection<Controller> devs)
        {
            mList = devs;
            Devices = new ObservableCollection<DeviceEditViewModel>(mList.Select(x => new DeviceEditViewModel(x)));
            Devices.CollectionChanged += Devices_CollectionChanged;
        }

        private void Devices_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Trigger();
        }

        public ObservableCollection<DeviceEditViewModel> Devices { get; }
        public DeviceEditViewModel? Selected { get; set; }
        public bool CanRemove => Devices.Any() && (Selected?.CanEdit ?? false);

        public void Add()
        {
            var cfg = new ControllerConfig
            {
                PortName = SerialPortStream.GetPortNames().FirstOrDefault() ?? string.Empty
            };
            var c = new Controller(cfg);
            mList.Add(c);
            Devices.Add(new DeviceEditViewModel(c));
            Trigger();
        }
        public void Remove(DeviceEditViewModel? item)
        {
            if (item == null) return;
            Devices.Remove(item);
            mList.Remove(item.Instance);
            Trigger();
        }
        public void Trigger()
        {
            RaisePropertyChanged(nameof(CanRemove));
            Dispatcher.UIThread.Post(() => ListChanged?.Invoke(this, new EventArgs()));
        }
    }
}

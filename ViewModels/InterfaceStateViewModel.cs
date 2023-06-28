using ModbusRegisterMap;
using System;
using System.Threading.Tasks;
using ReactorControl.Models;

namespace ReactorControl.ViewModels
{
    public class InterfaceStateViewModel : ViewModelBase
    {
        protected Controller mController;

        public InterfaceStateViewModel(Controller c)
        {
            mController = c;
            if (c.RegisterMap.HoldingRegisters[Constants.InterfaceActivityName] is not Register<DevUShort> reg) 
                throw new Exception("Register 'InterfaceActivityName' can not be found.");
            Register = reg;
            Register.PropertyChanged += Register_PropertyChanged;
        }

        private void Register_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsReceiving));
        }

        public Register<DevUShort> Register { get; }
        public bool IsReceiving => ((Constants.InterfaceActivityBits)Register.TypedValue.Value)
            .HasFlag(Constants.InterfaceActivityBits.Receive);

        protected DevUShort SetFlag(Constants.InterfaceActivityBits b, bool set = true)
        {
            ushort v = Register.TypedValue.Value;
            v &= (ushort)Constants.InterfaceActivityBits.Receive; //Remove any single-shot bits still not cleared
            if (set) v |= (ushort)b;
            else v &= (ushort)(~(ushort)b);
            return (DevUShort)v;
        }
        
        public async Task RemoteControl(bool receive)
        {
            await mController.WriteRegister(Constants.InterfaceActivityName, 
                SetFlag(Constants.InterfaceActivityBits.Receive, receive));
            await mController.ReadRegister(Constants.InterfaceActivityName);
        }
        public async Task ReloadParams()
        {
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.Reload));
        }
        public async Task SaveNVS()
        {
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.SaveNVS));
        }
        public async Task Reset()
        {
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.Reboot));
        }
    }
}

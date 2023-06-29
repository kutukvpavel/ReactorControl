﻿using ModbusRegisterMap;
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
            Register = c.RegisterMap.HoldingRegisters[Constants.InterfaceActivityName] as Register<DevUShort>;
            if (Register == null) return;
            Register.PropertyChanged += Register_PropertyChanged;
        }

        private void Register_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsReceiving));
        }

        public Register<DevUShort>? Register { get; }
        public bool? IsReceiving => Register == null ? null :
            ((Constants.InterfaceActivityBits)Register.TypedValue.Value).HasFlag(Constants.InterfaceActivityBits.Receive);

        protected DevUShort SetFlag(Constants.InterfaceActivityBits b, bool set = true)
        {
            if (Register == null) return (DevUShort)0;
            ushort v = Register.TypedValue.Value;
            v &= (ushort)Constants.InterfaceActivityBits.Receive; //Remove any single-shot bits still not cleared
            if (set) v |= (ushort)b;
            else v &= (ushort)(~(ushort)b);
            return (DevUShort)v;
        }
        
        public async Task RemoteControl(bool receive)
        {
            if (Register == null) return;
            await mController.WriteRegister(Constants.InterfaceActivityName, 
                SetFlag(Constants.InterfaceActivityBits.Receive, receive));
            await mController.ReadRegister(Constants.InterfaceActivityName);
        }
        public async Task ReloadParams()
        {
            if (Register == null) return;
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.Reload));
        }
        public async Task SaveNVS()
        {
            if (Register == null) return;
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.SaveNVS));
        }
        public async Task Reset()
        {
            if (Register == null) return;
            await mController.WriteRegister(Constants.InterfaceActivityName, SetFlag(Constants.InterfaceActivityBits.Reboot));
        }
    }
}

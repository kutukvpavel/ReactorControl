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
            mController.PropertyChanged += MController_PropertyChanged;
        }

        private void MController_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mController.IsConnected)) 
            {
                Register = mController.RegisterMap.HoldingRegisters[Constants.InterfaceActivityName] as Register<DevUShort>;
                if (Register == null) return;
                Register.PropertyChanged += Register_PropertyChanged;
                RaisePropertyChanged(nameof(IsEnabled));
            }
            RaisePropertyChanged(nameof(IsReceiving));
            RaisePropertyChanged(nameof(CanControl));
            RaisePropertyChanged(nameof(CanDFU));
            RaisePropertyChanged(nameof(CanReset));
        }

        private void Register_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsReceiving));
            RaisePropertyChanged(nameof(CanControl));
            RaisePropertyChanged(nameof(CanReset));
        }

        public Register<DevUShort>? Register { get; private set; }
        public bool IsReceiving => mController.IsRemoteEnabled;
        public bool IsEnabled => mController.IsConnected;
        public bool CanControl => IsEnabled && IsReceiving && (mController.Mode == Constants.Modes.Auto);
        public bool CanDFU => IsEnabled && IsReceiving && (mController.Mode == Constants.Modes.Init);
        public bool CanReset => IsEnabled && IsReceiving;

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
            await mController.SetRemoteControl(receive);
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
        public async Task EnterDFU()
        {
            if (Register == null) return;
            await mController.EnterDFU();
        }
    }
}

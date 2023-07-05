using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ModbusRegisterMap;
using ReactorControl.Models;

namespace ReactorControl.ViewModels
{
    public class PumpControlViewModel : ViewModelBase
    {
        public static string SpeedNumberFormat { get; set; } = "F3";
        public static string NotAvailable { get; set; } = "N/A";

        protected Controller mController;

        public PumpControlViewModel(Controller c, int index)
        {
            mController = c;
            Index = index;
            MotorReg = mController.RegisterMap.InputRegisters[Constants.MotorRegistersBaseName + Index.ToString()]
                as Register<DevMotorReg>;
            MotorParams = mController.RegisterMap.HoldingRegisters[Constants.MotorParamsBaseName + Index.ToString()]
                as Register<DevMotorParams>;
            CommandedSpeedRegister = mController.RegisterMap.HoldingRegisters[Constants.CommandedSpeedBaseName + Index.ToString()]
                as Register<DevFloat>;
            mController.PropertyChanged += Controller_PropertyChanged;
            if (MotorReg != null) MotorReg.PropertyChanged += MotorReg_PropertyChanged;
            if (MotorParams != null) MotorParams.PropertyChanged += MotorParams_PropertyChanged;
            if (CommandedSpeedRegister != null) CommandedSpeedRegister.PropertyChanged += CommandedSpeedRegister_PropertyChanged;
        }

        private void Controller_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mController.IsRemoteEnabled)
                || e.PropertyName == nameof(mController.Mode))
            {
                RaisePropertyChanged(nameof(CanEdit));
                RaisePropertyChanged(nameof(CommandedColor));
            }
        }
        private void CommandedSpeedRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CommandedSpeed));
        }
        private void MotorParams_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
        private void MotorReg_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MotorStatus));
            RaisePropertyChanged(nameof(VolumeRate));
            RaisePropertyChanged(nameof(RotationSpeed));
            RaisePropertyChanged(nameof(Load));
            RaisePropertyChanged(nameof(StatusString));
            RaisePropertyChanged(nameof(StatusColor));
        }

        public int Index { get; }
        public string IndexString => $"Pump #{Index}";
        public string VolumeRateUnit => mController.Config.VolumeRateUnit;
        public Register<DevMotorReg>? MotorReg { get; }
        public Register<DevMotorParams>? MotorParams { get; }
        public Register<DevFloat>? CommandedSpeedRegister { get; }
        public Constants.MotorStatusBits? MotorStatus => (Constants.MotorStatusBits?)MotorReg?.TypedValue.Status.Value;
        public string? VolumeRate => MotorReg?.TypedValue.VolumeRate.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public string? RotationSpeed => MotorReg?.TypedValue.RPS.Value.ToString("F4", CultureInfo.CurrentUICulture)
            ?? NotAvailable;
        public string? CommandedSpeed => CommandedSpeedRegister?.TypedValue.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public bool CanEdit => CommandedSpeedRegister != null && MotorReg != null && mController.IsRemoteEnabled;
        public string? Load => MotorReg == null ? NotAvailable :
            (MotorReg.TypedValue.Error.Value * 100).ToString("F0", CultureInfo.CurrentUICulture);
        public string StatusString
        {
            get
            {
                if (MotorStatus == null) return "Not Connected";
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Missing)) return "Missing!";
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Overload)) return "Overload!";
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Paused)) return "Paused";
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Running)) return "Running";
                return "N/A";
            }
        }
        public IBrush StatusColor
        {
            get
            {
                if (MotorStatus == null) return Brushes.LightGray;
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Missing)) return Brushes.LightSalmon;
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Overload)) return Brushes.LightCoral;
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Paused)) return Brushes.LightBlue;
                if (MotorStatus.Value.HasFlag(Constants.MotorStatusBits.Running)) return Brushes.LightGreen;
                return Brushes.LightGray;
            }
        }
        static protected readonly IBrush FaintGreen = (IBrush)new BrushConverter().ConvertFrom("#c6f5c6");
        public IBrush CommandedColor => ((mController.Mode == Constants.Modes.Auto) && mController.IsRemoteEnabled) ?
            FaintGreen : Brushes.LightGray;

        public async Task SetVolumeRate(float v)
        {
            if (CommandedSpeedRegister == null || MotorReg == null) return;
            CommandedSpeedRegister.TypedValue.Value = v; //Commanded registers are not polled, no concurrency expected
            await mController.WriteRegister(CommandedSpeedRegister);
            await Task.Delay(100);
            await mController.ReadRegister(CommandedSpeedRegister);
            if (mController.IsPolling) return;
            await mController.ReadRegister(MotorReg);
        }
    }
}

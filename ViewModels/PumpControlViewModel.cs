using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Media;
using ModbusRegisterMap;
using ReactorControl.Models;

namespace ReactorControl.ViewModels
{
    public class PumpControlViewModel : ViewModelBase
    {
        public static string TimeNumberFormat { get; set; } = "F1";
        public static string SpeedNumberFormat { get; set; } = "F3";
        public static string VolumeNumberFormat { get; set; } = "F2";
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
            CommandedTimeRegister = mController.RegisterMap.HoldingRegisters[Constants.CommandedTimerBaseName + Index.ToString()]
                as Register<DevFloat>;
            mController.PropertyChanged += Controller_PropertyChanged;
            if (MotorReg != null) MotorReg.PropertyChanged += MotorReg_PropertyChanged;
            if (MotorParams != null) MotorParams.PropertyChanged += MotorParams_PropertyChanged;
            if (CommandedSpeedRegister != null) CommandedSpeedRegister.PropertyChanged += CommandedSpeedRegister_PropertyChanged;
            if (CommandedTimeRegister != null) CommandedTimeRegister.PropertyChanged += CommandedTimeRegister_PropertyChanged;
        }

        private void Controller_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mController.IsRemoteEnabled)
                || e.PropertyName == nameof(mController.Mode))
            {
                RaisePropertyChanged(nameof(CanEdit));
                RaisePropertyChanged(nameof(CanChangeTimerMode));
                RaisePropertyChanged(nameof(CommandedColor));
            }
        }
        private void CommandedTimeRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CommandedTime));
            RaisePropertyChanged(nameof(CommandedVolume));
        }
        private void CommandedSpeedRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CommandedSpeed));
            RaisePropertyChanged(nameof(CommandedVolume));
        }
        private void MotorParams_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
        private void MotorReg_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MotorStatus));
            RaisePropertyChanged(nameof(TimerEnabled));
            RaisePropertyChanged(nameof(VolumeRate));
            RaisePropertyChanged(nameof(RotationSpeed));
            RaisePropertyChanged(nameof(Load));
            RaisePropertyChanged(nameof(StatusString));
            RaisePropertyChanged(nameof(StatusColor));
            RaisePropertyChanged(nameof(TimeLeft));
            RaisePropertyChanged(nameof(VolumeLeft));
        }

        private async Task SetRegister(Register<DevFloat>? r, float v)
        {
            if (r == null || MotorReg == null) return;
            r.TypedValue.Value = v; //Commanded registers are not polled, no concurrency expected
            await mController.WriteRegister(r);
            await Task.Delay(100);
            await mController.ReadRegister(r);
            if (mController.IsPolling) return;
            await mController.ReadRegister(MotorReg);
        }

        public int Index { get; }
        public string IndexString => $"Pump #{Index}";
        public string VolumeRateUnit => mController.Config.VolumeRateUnit;
        public string VolumeUnit => mController.Config.VolumeUnit;
        public Register<DevMotorReg>? MotorReg { get; }
        public Register<DevMotorParams>? MotorParams { get; }
        public Register<DevFloat>? CommandedSpeedRegister { get; }
        public Register<DevFloat>? CommandedTimeRegister { get; }
        public Constants.MotorStatusBits? MotorStatus => (Constants.MotorStatusBits?)MotorReg?.TypedValue.Status.Value;
        public string? VolumeRate => MotorReg?.TypedValue.VolumeRate.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public string? RotationSpeed => MotorReg?.TypedValue.RPS.Value.ToString("F4", CultureInfo.CurrentUICulture)
            ?? NotAvailable;
        public string? CommandedSpeed => CommandedSpeedRegister?.TypedValue.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public string? CommandedTime => CommandedTimeRegister?.TypedValue.Value
            .ToString(TimeNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public string? CommandedVolume 
        {
            get {
                if (CommandedSpeedRegister?.TypedValue is null) return NotAvailable;
                if (CommandedTimeRegister?.TypedValue is null) return NotAvailable;
                return (CommandedSpeedRegister.TypedValue.Value * CommandedTimeRegister.TypedValue.Value)
                    .ToString(VolumeNumberFormat, CultureInfo.CurrentUICulture);
            }
        }
        public string? TimeLeft => MotorReg?.TypedValue.RunTimeLeft.Value
            .ToString(TimeNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public string? VolumeLeft 
        {
            get {
                if (MotorReg == null) return NotAvailable;
                return (MotorReg.TypedValue.RunTimeLeft.Value * MotorReg.TypedValue.VolumeRate.Value)
                    .ToString(VolumeNumberFormat, CultureInfo.CurrentUICulture);
            }
        }
        public bool? TimerEnabled => MotorStatus?.HasFlag(Constants.MotorStatusBits.TimerMode);
        public bool CanEdit => CanChangeTimerMode &&
            ((!(MotorStatus?.HasFlag(Constants.MotorStatusBits.TimerTicking) ?? false) && (MotorStatus?.HasFlag(Constants.MotorStatusBits.TimerMode) ?? false)) || 
            (!(MotorStatus?.HasFlag(Constants.MotorStatusBits.TimerMode) ?? false)));
        public bool CanChangeTimerMode => CommandedSpeedRegister != null && MotorReg != null && mController.IsRemoteEnabled;
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
            await SetRegister(CommandedSpeedRegister, v);
        }
        public async Task SetTimer(float v)
        {
            await SetRegister(CommandedTimeRegister, v);
        }
        public async Task EnableTimer(bool v)
        {
            await mController.EnableTimer(Index, v);
        }
        public async Task RunTimer()
        {
            await mController.TriggerTimer(Index);
        }
    }
}

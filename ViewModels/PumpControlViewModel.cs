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
        public static string SpeedNumberFormat { get; set; } = "F3";
        public static string NotAvailable { get; set; } = "N/A";

        protected Controller mController;

        public PumpControlViewModel(Controller c, int index)
        {
            mController = c;
            Index = index;
            MotorReg = mController.RegisterMap.HoldingRegisters[Constants.MotorRegistersBaseName + Index.ToString()]
                as Register<DevMotorReg>;
            MotorParams = mController.RegisterMap.HoldingRegisters[Constants.MotorParamsBaseName + Index.ToString()]
                as Register<DevMotorParams>;
            CommandedSpeedRegister = mController.RegisterMap.HoldingRegisters[Constants.CommandedSpeedBaseName + Index.ToString()]
                as Register<DevFloat>;
        }

        public int Index { get; }
        public string IndexString => $"Pump #{Index}";
        public string VolumeRateUnit => mController.Config.VolumeRateUnit;
        public Register<DevMotorReg>? MotorReg { get; }
        public Register<DevMotorParams>? MotorParams { get; }
        public Register<DevFloat>? CommandedSpeedRegister { get; }
        public Constants.MotorStatusBits? MotorStatus => (Constants.MotorStatusBits?)MotorReg?.TypedValue.Status.Value;
        public string? VolumeRate => MotorReg?.TypedValue.VolumeRate.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture);
        public string? RotationSpeed => MotorReg?.TypedValue.RPS.Value.ToString("F4", CultureInfo.CurrentUICulture);
        public string? CommandedSpeed => CommandedSpeedRegister?.TypedValue.Value
            .ToString(SpeedNumberFormat, CultureInfo.CurrentUICulture) ?? NotAvailable;
        public bool CanEdit => CommandedSpeedRegister != null && MotorReg != null;
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
                return "OK";
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
                return Brushes.LightGreen;
            }
        }

        public async Task SetVolumeRate(float v)
        {
            if (CommandedSpeedRegister == null || MotorReg == null) return;
            CommandedSpeedRegister.TypedValue.Value = v; //Commanded registers are not polled, no concurrency expected
            await mController.WriteRegister(CommandedSpeedRegister);
            if (mController.IsPolling) return;
            await Task.Delay(100);
            await mController.ReadRegister(MotorReg);
        }
    }
}

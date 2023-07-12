using Avalonia.Media;
using ModbusRegisterMap;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ReactorControl.Models
{
    public class ProbeConfig
    {
        public static List<ProbeConfig> DefaultProbeConfig { get; } = new()
        {
            GetStandardProbeConfig("Thermocouple 1", Constants.ThermocoupleBaseName, 0),
            GetStandardProbeConfig("Thermocouple 2", Constants.ThermocoupleBaseName, 1),
            GetStandardProbeConfig("Thermocouple 3", Constants.AnalogInputBaseName,
                (int)Constants.AnalogInputs.Thermocouple),
            GetStandardProbeConfig("Ambient Temperature", Constants.AnalogInputBaseName,
                (int)Constants.AnalogInputs.AmbientTemperature)
        };

        public static IBrush DefaultStatusColorProvider(IDeviceType v)
        {
            return float.IsNaN(((DevFloat)v).Value) ? Brushes.LightGray : Brushes.LightGreen;
        }
        public static string DefaultStatusTextProvider(IDeviceType v)
        {
            return float.IsNaN(((DevFloat)v).Value) ? "Not Connected" : "OK";
        }
        public static ProbeConfig GetStandardProbeConfig(string displayName, string regName, int index = -1)
        {
            var r = new ProbeConfig(displayName, regName, index)
            {
                DefaultStatusString = "N/A",
                DefaultStatusColor = Brushes.LightGray,
                Units = "°C",
                StatusColorProvider = DefaultStatusColorProvider,
                StatusStringProvider = DefaultStatusTextProvider
            };
            switch (regName)
            {
                case Constants.AnalogInputBaseName:
                    r.TriggerPropertyName = nameof(Controller.TotalAnalogInputs);
                    break;
                case Constants.ThermocoupleBaseName:
                    r.TriggerPropertyName = nameof(Controller.TotalThermocouples);
                    break;
                default:
                    break;
            }
            return r;
        }

        public ProbeConfig()
        {

        }
        public ProbeConfig(string displayName, string regName, int index = -1) : this()
        {
            DisplayName = displayName;
            if (index >= 0) regName += index.ToString();
            RegisterName = regName;
        }

        public string RegisterName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Units { get; set; } = string.Empty;
        public string? DisplayFormat { get; set; }
        [YamlIgnore]
        public IBrush DefaultStatusColor { get; set; } = Brushes.Transparent;
        public string DefaultStatusString { get; set; } = "N/A";
        [YamlIgnore]
        public Func<IDeviceType, IBrush>? StatusColorProvider { get; set; } = null;
        [YamlIgnore]
        public Func<IDeviceType, string>? StatusStringProvider { get; set; } = null;
        public string? TriggerPropertyName { get; set; }

        public IBrush GetStatusColor(IRegister? reg)
        {
            if (StatusColorProvider == null || reg == null) return DefaultStatusColor;
            try
            {
                return StatusColorProvider(reg.Value);
            }
            catch (Exception)
            {
                return DefaultStatusColor;
            }
        }
        public string GetStatusString(IRegister? reg)
        {
            if (StatusStringProvider == null || reg == null) return DefaultStatusString;
            try
            {
                return StatusStringProvider(reg.Value);
            }
            catch (Exception)
            {
                return DefaultStatusString;
            }
        }
    }
}

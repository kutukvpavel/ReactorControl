using System;
using System.Drawing;
using ModbusRegisterMap;
using YamlDotNet.Serialization;

namespace ReactorControl.Models
{
    public class DevPumpParams : ComplexDevTypeBase
    {
        public DevPumpParams()
        {
            Fields = new IDeviceType[] {
                InvertEnable,
                Reserved,
                VolumeRateResolution
            };
        }

        public DevUShort InvertEnable { get; set; } = new();
        public DevUShort Reserved { get; set; } = new();
        public DevFloat VolumeRateResolution { get; set; } = new();

        [YamlIgnore]
        public override ushort Size => 1 + 1 + 2;
        public override object Get()
        {
            return this;
        }
        public override string ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class DevMotorParams : ComplexDevTypeBase
    {
        public DevMotorParams()
        {
            Fields = new IDeviceType[] {
                Direction,
                Microsteps,
                Teeth,
                Reserved,
                VolumeRateToRPS,
                MaxRateRPS,
                MaxLoadError
            };
        }

        public DevUShort Direction {get;set;} = new();
        public DevUShort Microsteps {get;set;} = new();
        public DevUShort Teeth {get;set;} = new();
        public DevUShort Reserved {get;set;} = new();
        public DevFloat VolumeRateToRPS {get;set;} = new();
        public DevFloat MaxRateRPS {get;set;} = new();
        public DevFloat MaxLoadError {get;set;} = new();

        [YamlIgnore]
        public override ushort Size => 1 * 4 + 2 * 3;
        public override object Get()
        {
            return this;
        }
        public override string ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class DevMotorReg : ComplexDevTypeBase
    {
        public DevMotorReg()
        {
            Fields = new IDeviceType[] {
                VolumeRate,
                RPS,
                Error,
                Status
            };
        }

        public DevFloat VolumeRate { get; set; } = new();
        public DevFloat RPS { get; set; } = new();
        public DevFloat Error { get; set; } = new();
        public DevUShort Status { get; set; } = new();
        //public ushort Reserved {get;set;}

        [YamlIgnore]
        public override ushort Size => 2 + 2 + 2 + 1 + 1;
        public override object Get()
        {
            return this;
        }
        public override string ToString()
        {
            return FormattableString.Invariant($"{VolumeRate},{RPS},{Error},{Status}");
        }
        public override string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3}", VolumeRate, RPS, Error, Status);
        }
    }
}
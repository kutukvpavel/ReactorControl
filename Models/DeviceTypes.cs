using System;
using ModbusRegisterMap;

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

        public override ushort Size => 1 + 1 + 2;
        public override object Get()
        {
            return this;
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

        public override ushort Size => 1 * 4 + 2 * 3;
        public override object Get()
        {
            return this;
        }
    }

    public class DevMotorRegs : ComplexDevTypeBase
    {
        public DevMotorRegs()
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

        public override ushort Size => 2 + 2 + 2 + 1 + 1;
        public override object Get()
        {
            return this;
        }
    }
}
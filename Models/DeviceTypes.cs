using System;
using ModbusRegisterMap;
using YamlDotNet.Serialization;

namespace ReactorControl.Models
{
    public class DevPumpParams : ComplexDevTypeBase
    {
        public DevPumpParams()
        {
            Fields = new IDeviceType[] {
                _InvertEnable,
                _Reserved,
                _VolumeRateResolution
            };
        }

        readonly DevUShort _InvertEnable = new();
        public DevUShort InvertEnable
        {
            get => _InvertEnable;
            set {
                _InvertEnable.Value = value.Value;
            }
        }

        readonly DevUShort _Reserved = new();
        public DevUShort Reserved
        {
            get => _Reserved;
            set {
                _Reserved.Value = value.Value;
            }
        }

        readonly DevFloat _VolumeRateResolution = new();
        public DevFloat VolumeRateResolution
        {
            get => _VolumeRateResolution;
            set {
                _VolumeRateResolution.Value = value.Value;
            }
        }

        /*[YamlIgnore]
        public override ushort Size => 1 + 1 + 2;*/
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
                _Direction,
                _Microsteps,
                _Teeth,
                _Reserved,
                _VolumeRateToRPS,
                _MaxRateRPS,
                _MaxLoadError
            };
        }

        readonly DevUShort _Direction = new();
        public DevUShort Direction
        {
            get => _Direction;
            set {
                _Direction.Value = value.Value;
            }
        }

        readonly DevUShort _Microsteps = new();
        public DevUShort Microsteps
        {
            get => _Microsteps;
            set {
                _Microsteps.Value = value.Value;
            }
        }

        readonly DevUShort _Teeth = new();
        public DevUShort Teeth
        {
            get => _Teeth;
            set {
                _Teeth.Value = value.Value;
            }
        }

        readonly DevUShort _Reserved = new();
        public DevUShort Reserved
        {
            get => _Reserved;
            set {
                _Reserved.Value = value.Value;
            }
        }

        readonly DevFloat _VolumeRateToRPS = new();
        public DevFloat VolumeRateToRPS
        {
            get => _VolumeRateToRPS;
            set {
                _VolumeRateToRPS.Value = value.Value;
            }
        }

        readonly DevFloat _MaxRateRPS = new();
        public DevFloat MaxRateRPS
        {
            get => _MaxRateRPS;
            set {
                _MaxRateRPS.Value = value.Value;
            }
        }

        readonly DevFloat _MaxLoadError = new();
        public DevFloat MaxLoadError
        {
            get => _MaxLoadError;
            set {
                _MaxLoadError.Value = value.Value;
            }
        }

        /*[YamlIgnore]
        public override ushort Size => 1 * 4 + 2 * 3;*/
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
                _VolumeRate,
                _RPS,
                _Error,
                _Status,
                _Reserved1,
                _RunTimeLeft
            };
        }

        readonly DevFloat _VolumeRate = new();
        public DevFloat VolumeRate
        {
            get => _VolumeRate;
            set {
                _VolumeRate.Value = value.Value;
            }
        }
        readonly DevFloat _RPS = new();
        public DevFloat RPS
        {
            get => _RPS;
            set {
                _RPS.Value = value.Value;
            }
        }
        readonly DevFloat _Error = new();
        public DevFloat Error
        {
            get => _Error;
            set {
                _Error.Value = value.Value;
            }
        }
        readonly DevUShort _Status = new();
        public DevUShort Status
        {
            get => _Status;
            set {
                _Status.Value = value.Value;
            }
        }
        readonly DevUShort _Reserved1 = new();
        public DevUShort Reserved1
        {
            get => _Reserved1;
            set {
                _Reserved1.Value = value.Value;
            }
        }
        readonly DevFloat _RunTimeLeft = new();
        public DevFloat RunTimeLeft
        {
            get => _RunTimeLeft;
            set {
                _RunTimeLeft.Value = value.Value;
            }
        }
        

        /*[YamlIgnore]
        public override ushort Size => 2 + 2 + 2 + 1 + 1 + 2;*/
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
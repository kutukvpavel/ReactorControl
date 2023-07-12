using System;

namespace ReactorControl.Models
{
    public static class Constants
    {
        public enum Modes : ushort
        {
            Init = 0,
            Manual,
            Auto,
            Emergency,
            LampTest,

            NA = 0xFFFF
        }
        [Flags]
        public enum InterfaceActivityBits : ushort
        {
            Receive = 1,
            Reload = 2,
            SaveNVS = 4,
            Reboot = 8,
            KeepAlive = 16,
            Reserved1 = 32,
            FirmwareUpgrade = 64
        }
        [Flags]
        public enum MotorStatusBits : ushort
        {
            Missing = 1,
            Overload = 2,
            Paused = 4,
            Running = 8
        }

        //Config
        public const string PumpsNumName = "PUMPS_NUM";
        public const string ThermocouplesNumName = "THERMO_NUM";
        public const string InputWordsName = "IN_LEN";
        public const string OutputWordsName = "OUT_LEN";
        public const string AnalogInputNumName = "AIO_NUM";

        //Holding
        public const string StatusRegisterName = "STATUS";
        public const string InterfaceActivityName = "IF_ACT";
        public const string ModbusAddrName = "ADDR";
        public const string ThermocoupleBaseName = "THERMO_";
        public const string AnalogInputBaseName = "AIO_";
        public const string AnalogCalBaseName = "AIO_CAL_";
        public const string InputsRegisterBaseName = "IN_";
        public const string OutputsRegisterBaseName = "OUT_";
        public const string CommandedOutputsBaseName = "COMMANDED_OUT_";
        public const string PumpParamsName = "PUMP_PARAMS";
        public const string MotorParamsBaseName = "MOTOR_PARAMS_";
        public const string MotorRegistersBaseName = "MOTOR_REGS_";
        public const string CommandedSpeedBaseName = "COMMANDED_SPEED_";

        public enum ConfigRegisters
        {
            PumpsNum = 0,
            ThermocouplesNum,
            InputLength,
            OutputLength,
            AioNum,

            LEN
        }
        public static readonly string[] ConfigRegisterNames = 
        {
            PumpsNumName,
            ThermocouplesNumName,
            InputWordsName,
            OutputWordsName,
            AnalogInputNumName
        };

        public enum AnalogInputs
        {
            AmbientTemperature,
            Thermocouple,
            Vref,
            Vbat,

            LEN
        }
    }
}
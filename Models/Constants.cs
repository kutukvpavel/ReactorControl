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
            KeepAlive = 16
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
        public static readonly string PumpsNumName = "PUMPS_NUM";
        public static readonly string ThermocouplesNumName = "THERMO_NUM";
        public static readonly string InputWordsName = "IN_LEN";
        public static readonly string OutputWordsName = "OUT_LEN";
        public static readonly string AnalogInputNumName = "AIO_NUM";

        //Holding
        public static readonly string StatusRegisterName = "STATUS";
        public static readonly string InterfaceActivityName = "IF_ACT";
        public static readonly string ModbusAddrName = "ADDR";
        public static readonly string ThermocoupleBaseName = "THERMO_";
        public static readonly string AnalogInputBaseName = "AIO_";
        public static readonly string AnalogCalBaseName = "AIO_CAL_";
        public static readonly string InputsRegisterBaseName = "IN_";
        public static readonly string OutputsRegisterBaseName = "OUT_";
        public static readonly string CommandedOutputsBaseName = "COMMANDED_OUT_";
        public static readonly string PumpParamsName = "PUMP_PARAMS";
        public static readonly string MotorParamsBaseName = "MOTOR_PARAMS_";
        public static readonly string MotorRegistersBaseName = "MOTOR_REGS_";
        public static readonly string CommandedSpeedBaseName = "COMMANDED_SPEED_";

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
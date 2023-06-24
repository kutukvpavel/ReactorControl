using System;

namespace ReactorControl.Models
{
    public static class Constants
    {
        [Flags]
        public enum InterfaceActivityBits : ushort
        {
            Receive = 1,
            Reload = 2,
            SaveNVS = 4,
            Reboot = 8
        }

        public static readonly string PumpsNumName = "PUMPS_NUM";
        public static readonly string StatusRegisterName = "STATUS";
        public static readonly string InterfaceActivityName = "IF_ACT";
        public static readonly string ThermocoupleBaseName = "THERMO_";
        public static readonly string ModbusAddrName = "ADDR";
        public static readonly string InputsRegisterName = "IN";
        public static readonly string OutputsRegisterName = "OUT";
        public static readonly string CommandedOutputsName = "COMMANDED_OUT";
        public static readonly string PumpParamsName = "PUMP_PARAMS";
        public static readonly string MotorParamsBaseName = "MOTOR_PARAMS_";
        public static readonly string MotorRegistersBaseName = "MOTOR_REGS_";
        public static readonly string CommandedSpeedBaseName = "COMMANDED_SPEED_";

        public enum ConfigRegisters
        {
            PumpsNum = 0,

            LEN
        }
        public static readonly string[] ConfigRegisterNames = 
        {
            PumpParamsName
        };
    }
}
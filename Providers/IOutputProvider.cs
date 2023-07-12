using ModbusRegisterMap;
using System;
using System.Text;

namespace ReactorControl.Providers
{
    public interface IOutputProvider
    {
        public static string YamlStartFlag { get; set; } = "---";
        public static string YamlEndFlag { get; set; } = "...";
        public static Encoding PacketEncoding { get; set; } = Encoding.UTF8;

        public event EventHandler<LogEventArgs>? LogEvent;

        public void SendTest();
        public void Send(IRegister v);
        public void Connect();
        public void Disconnect();
    }
}

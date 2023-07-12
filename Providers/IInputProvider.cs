using System;
using YamlDotNet.Serialization;

namespace ReactorControl.Providers
{
    public interface IInputProvider
    {
        public static string YamlStartFlag { get; set; } = "---";
        public static string YamlEndFlag { get; set; } = "...";
        protected static Deserializer DeserializerInstance { get; } = new Deserializer();

        public event EventHandler<LogEventArgs>? LogEvent;

        public event EventHandler<IpcEventArgs>? CommandReceived;
        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;
    }

    public class IpcEventArgs
    {
        public IpcEventArgs()
        {

        }

        public string? SourceTag { get; set; }
        public string? TargetDeviceName { get; set; }
        public string? RegisterName { get; set; }
        public string? ValueString { get; set; }
    }
}

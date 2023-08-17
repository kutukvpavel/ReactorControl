using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using YamlDotNet.Serialization;

namespace ReactorControl.Providers
{
    public class ScriptProvider : IInputProvider
    {
        public class Command
        {
            public Command()
            {

            }


        }

        public event EventHandler<LogEventArgs>? LogEvent;
        public event EventHandler<IpcEventArgs>? CommandReceived;
        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;

        public ScriptProvider(string filePath)
        {
            //IInputProvider.DeserializerInstance.Deserialize<>(File.ReadAllText(filePath));
        }

        protected Timer mTick = new Timer(1000) { AutoReset = true, Enabled = false };

        public List<Command> Commands { get; set; } = new List<Command>();
    }
}

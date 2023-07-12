using ModbusRegisterMap;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ReactorControl.Providers
{
    public abstract class OutputProviderBase : IOutputProvider
    {
        public static Serializer SerializerInstance { get; } = new Serializer();

        public event EventHandler<LogEventArgs>? LogEvent;

        public class Data
        {
            public Data(string reg, string? v)
            {
                Timestamp = DateTime.Now.Ticks;
                RegisterName = reg;
                ValueString = v ?? string.Empty;
            }

            public long Timestamp { get; set; }
            public string RegisterName { get; set; }
            public string ValueString { get; set; }
        }

        protected OutputProviderBase(string tag)
        {
            Tag = tag;
            CancellationSource = new();
        }

        protected CancellationTokenSource CancellationSource { get; private set; }
        protected Task? QueueHandlerTask { get; private set; }
        protected bool IsConnected { get; private set; } = false;

        public BlockingCollection<Data> Messages { get; } = new();
        public string Tag { get; set; }

        protected void QueueHandler()
        {
            while (!CancellationSource.IsCancellationRequested)
            {
                try
                {
                    var res = Messages.Take(CancellationSource.Token);
                    string s = ConvertData(res);
                    Send(s);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, new LogEventArgs(ex,
                        $"Failed to write to output provider {GetType().Name} '{Tag}'"));
                }
            }
        }
        protected void Log(string msg, Exception? ex = null)
        {
            LogEvent?.Invoke(this, new LogEventArgs(ex, msg));
        }
        protected abstract void Send(string data);
        protected virtual string ConvertData(Data d)
        {
            return $"{IOutputProvider.YamlStartFlag}\n{SerializerInstance.Serialize(d)}\n{IOutputProvider.YamlEndFlag}";
        }
        public void Send(IRegister v)
        {
            Messages.Add(new Data(v.Name, v.Value.ToString(null, CultureInfo.InvariantCulture)));
        }
        public virtual void Connect()
        {
            CancellationSource = new CancellationTokenSource();
            QueueHandlerTask = Task.Run(() => QueueHandler());
            IsConnected = true;
        }
        public virtual void Disconnect()
        {
            IsConnected = false;
            CancellationSource.Cancel();
            while ((QueueHandlerTask?.Status ?? TaskStatus.Canceled) == TaskStatus.Running) Thread.Sleep(1);
        }
        public void SendTest()
        {
            Messages.Add(new Data(string.Empty, null));
        }
    }
}

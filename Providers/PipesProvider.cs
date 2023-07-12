using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using YamlDotNet.Serialization;

namespace ReactorControl.Providers
{
    public class PipesProvider : OutputProviderBase, IInputProvider
    {
        public PipesProvider(string tag) : base(tag)
        {

        }

        NamedPipeServerStream? Pipe;
        TextWriter? Writer;
        TextReader? Reader;
        StringBuilder Buffer = new();

        public event EventHandler<IpcEventArgs>? CommandReceived;
        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;

        protected void ReceiveHandler()
        {
            if (Reader == null)
            {
                Log($"Pipe reader not initialized: '{Tag}'");
                return;
            }
            bool gotDocument = false;
            while (!CancellationSource.IsCancellationRequested)
            {
                try
                {
                    string? read = Reader.ReadLine();
                    if (gotDocument)
                    {
                        if (read == IInputProvider.YamlEndFlag)
                        {
                            Receive(Buffer.ToString());
                            Buffer.Clear();
                            gotDocument = false;
                        }
                        else
                        {
                            Buffer.Append(read);
                            Buffer.Append('\n');
                        }
                    }
                    else
                    {
                        if (read == IInputProvider.YamlStartFlag) gotDocument = true;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Pipe '{Tag}' receiver failed", ex);
                }
            }
        }
        protected void Receive(string data)
        {
            try
            {
                var res = IInputProvider.DeserializerInstance.Deserialize<IpcEventArgs>(data);
                CommandReceived?.Invoke(this, res);
            }
            catch (Exception ex)
            {
                Log("Failed to deserialize or execute received command from '{Tag}' input pipe", ex);
            }
        }
        protected override void Send(string data)
        {
            if (Writer == null) throw new NullReferenceException("Pipe server not initialized");
            Writer.Write(data + "\n\n");
        }
        public override void Connect()
        {
            if (IsConnected) Disconnect();
            try
            {
                Pipe = new NamedPipeServerStream($"ReactorCtrl_{Tag}", PipeDirection.InOut);
                Writer = new StreamWriter(Pipe, IOutputProvider.PacketEncoding);
                Reader = new StreamReader(Pipe, IOutputProvider.PacketEncoding);
                base.Connect();
            }
            catch (Exception ex)
            {
                Log($"Failed to open output pipe '{Tag}'", ex);
            }
        }
        public override void Disconnect()
        {
            if (!IsConnected) return;
            base.Disconnect();
            Reader?.Close();
            Reader?.Dispose();
            Writer?.Close();
            Writer?.Dispose();
            Pipe?.Close();
            Pipe?.Dispose();
        }
    }
}

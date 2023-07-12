using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactorControl.Providers
{
    public class SocketProvider : OutputProviderBase, IInputProvider
    {
        public static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        protected static readonly string YamlStarter = IInputProvider.YamlStartFlag + '\n';
        protected static readonly string YamlStopper = IInputProvider.YamlEndFlag + '\n';

        protected class Connection
        {
            public event EventHandler? Disconnected;
            public event EventHandler<LogEventArgs>? LogEvent;
            public event EventHandler<IpcEventArgs>? CommandReceived;

            public Connection(SocketProvider p, Socket s, CancellationToken t)
            {
                Parent = p;
                Instance = s;
                Token = t;
                Tag = $"{p.Tag}_{s.RemoteEndPoint}";
                ReceiverTask = Task.Run(() => ReceiveHandler());
            }

            public SocketProvider Parent { get; }
            public CancellationToken Token { get; }
            public bool GotDocument { get; set; } = false;
            public StringBuilder Buffer { get; } = new StringBuilder();
            public Socket Instance { get; }
            public string Tag { get; }
            public Task ReceiverTask { get; }

            protected void ProcessBufferAndInvoke()
            {
                string data = Buffer.ToString().Trim('\n');
                var res = IInputProvider.DeserializerInstance.Deserialize<IpcEventArgs>(data);
                CommandReceived?.Invoke(Parent, res);
            }
            protected void ReceiveHandler()
            {
                while (!Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!SocketConnected(Instance))
                        {
                            Disconnected?.Invoke(this, new EventArgs());
                            return;
                        }
                        byte[] b = new byte[Instance.Available];
                        int l = Instance.Receive(b, 0, b.Length, SocketFlags.None);
                        string r = IOutputProvider.PacketEncoding.GetString(b, 0, l);
                        Buffer.Append(r);
                        if (GotDocument)
                        {
                            int i = Buffer.ToString().IndexOf(YamlStopper);
                            if (i < 0) continue;
                            Buffer.Remove(i, Buffer.Length - i);
                            ProcessBufferAndInvoke();
                            GotDocument = false;
                            Buffer.Clear();
                        }
                        else
                        {
                            int i = Buffer.ToString().IndexOf(YamlStarter);
                            if (i < 0) continue;
                            Buffer.Remove(0, i + YamlStarter.Length);
                            GotDocument = true;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke(this, new LogEventArgs(ex, $"Failed to receive from input socket for '{Tag}'"));
                    }
                }
            }

            public void Send(string data)
            {
                try
                {
                    Instance.Send(IOutputProvider.PacketEncoding.GetBytes(data));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, new LogEventArgs(ex, $"Failed to send data through output socket '{Tag}'"));
                }
            }
        }

        public SocketProvider(string tag, IPEndPoint addr) : base(tag)
        {
            Address = addr;
        }

        public event EventHandler<IpcEventArgs>? CommandReceived;
        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;

        Socket? Socket;
        Task? ReceiveHandlerTask;
        readonly List<Connection> Accepted = new();

        public IPEndPoint Address { get; }

        protected void AcceptHandler()
        {
            if (Socket == null)
            {
                Log($"Socket for '{Tag}' is not initialized");
                return;
            }
            while (!CancellationSource.IsCancellationRequested)
            {
                try
                {
                    var s = Socket.Accept();
                    var c = new Connection(this, s, CancellationSource.Token);
                    c.LogEvent += C_LogEvent;
                    c.Disconnected += C_Disconnected;
                    c.CommandReceived += C_CommandReceived;
                    lock (Accepted)
                    {
                        Accepted.Add(c);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Failed to accept a connection to input socket for '{Tag}'", ex);
                }
            }
        }

        private void C_CommandReceived(object? sender, IpcEventArgs e)
        {
            CommandReceived?.Invoke(this, e);
        }
        private void C_Disconnected(object? sender, EventArgs e)
        {
            Connection? c = sender as Connection ?? 
                throw new NullReferenceException("Socket disconnect event sent by a wrong class");
            c.LogEvent -= C_LogEvent;
            c.CommandReceived -= C_CommandReceived;
            c.Disconnected -= C_Disconnected;
            lock (Accepted)
            {
                Accepted.Remove(c);
            }
        }
        private void C_LogEvent(object? sender, LogEventArgs e)
        {
            Log(e.Message, e.Exception);
        }

        protected override void Send(string data)
        {
            lock (Accepted)
            {
                foreach (var item in Accepted)
                {
                    item.Send(data);
                }
            }
        }
        public override void Connect()
        {
            if (IsConnected) Disconnect();
            try
            {
                Socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.Bind(Address);
                Socket.Listen(100);
                ReceiveHandlerTask = Task.Run(() => AcceptHandler());
                base.Connect();
            }
            catch (Exception ex)
            {
                Log($"Failed to set up socket for '{Tag}'", ex);
            }
        }
        public override void Disconnect()
        {
            if (!IsConnected) return;
            base.Disconnect();
            try
            {
                foreach (var item in Accepted)
                {
                    item.LogEvent -= C_LogEvent;
                    item.CommandReceived -= C_CommandReceived;
                    item.Disconnected -= C_Disconnected;
                }
                lock (Accepted)
                {
                    Accepted.Clear();
                }
                try
                {
                    Socket?.Disconnect(false);
                }
                catch (SocketException)
                {

                }
                Socket?.Close();
                Socket?.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
            try
            {
                if (!(ReceiveHandlerTask?.Wait(1000) ?? false)) throw new TimeoutException();
                foreach (var item in Accepted)
                {
                    if (!item.ReceiverTask.Wait(1000)) throw new TimeoutException();
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                Log($"Failed to terminate socket receier process for '{Tag}'", ex);
            }
        }
    }
}

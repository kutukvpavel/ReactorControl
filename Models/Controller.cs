using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModbusRegisterMap;
using NModbus;
using NModbus.SerialPortStream;
using RJCP.IO.Ports;
using Nito.AsyncEx;

using Timer = System.Timers.Timer;
using System.Collections;

namespace ReactorControl.Models
{
    public class Controller : INotifyPropertyChanged, IDisposable
    {
        public class PollResult
        {
            public PollResult()
            {
                Timestamp = DateTime.Now;
            }

            public DateTime Timestamp { get; }
        }

        #region Private

        protected readonly AsyncLock LockObject = new();
        protected SerialPortStreamAdapter? Adapter;
        protected IModbusMaster? Master;
        protected readonly ModbusFactory Factory = new();
        protected readonly Timer PollTimer;
        protected bool _IsConnected = false;

        private void PollTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(LockObject)) return;
            try
            {
                var task = Poll();
                task.Wait();
                if (task.IsCompletedSuccessfully)
                {
                    new Thread(() =>
                    {
                        PollCompleted?.Invoke(this, task.Result);
                    }).Start();
                }
            }
            catch (TimeoutException)
            {
                OnUnexpectedDisconnect();
            }
            catch (Exception)
            {

            }
            finally
            {
                Monitor.Exit(LockObject);
            }
        }
        protected async Task<bool> InitRegisterMap()
        {
            try
            {
                if (Master == null) throw new Exception("Controller connection does not exist.");
                //Add coils first
                //None

                RegisterMap.Clear();
                //Add configuration registers by default, build configuration dependent layout later
                for (int i = 0; i < (int)Constants.ConfigRegisters.LEN; i++)
                {
                    RegisterMap.AddInput<DevUShort>(Constants.ConfigRegisterNames[i], 1, true);
                }

                //Read configuration registers
                foreach (var item in RegisterMap.ConfigRegisters)
                {
                    if (RegisterMap.InputRegisters[item] is not IRegister reg)
                        throw new Exception($"Unknown register: {item}");
                    var read = await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length);
                    reg.Set(read);
                }
                //Build complete register map
                int pumpsTotal = RegisterMap.GetInputWord(Constants.PumpsNumName);
                int thermoTotal = RegisterMap.GetInputWord(Constants.ThermocouplesNumName);
                int inputWords = RegisterMap.GetInputWord(Constants.InputWordsName);
                int outputWords = RegisterMap.GetInputWord(Constants.OutputWordsName);
                int analogTotal = RegisterMap.GetInputWord(Constants.AnalogInputNumName);
                Log(null, $@"Device connected, config info:
    Number of pumps = {pumpsTotal}
    Number of thermocouples connected = {thermoTotal}
    Input register len = {inputWords}
    Output register len = {outputWords}
    Analog inputs = {analogTotal}");
                if (analogTotal > (int)Constants.AnalogInputs.LEN)
                    Log(null, $"Warning: more analog inputs then defined for this software version have been detected. Update required?");
                else if (analogTotal < (int)Constants.AnalogInputs.LEN)
                    throw new Exception("Detected less analog inputs then defined for this version of software. Cannot continue.");

                RegisterMap.AddInput<DevUShort>(Constants.StatusRegisterName, 1, true);
                RegisterMap.AddHolding<DevUShort>(Constants.InterfaceActivityName, 1, true);
                RegisterMap.AddHolding<DevUShort>(Constants.ModbusAddrName, 1);
                RegisterMap.AddInput<DevFloat>(Constants.ThermocoupleBaseName, thermoTotal, true);
                RegisterMap.AddInput<DevFloat>(Constants.AnalogInputBaseName, analogTotal, true);
                RegisterMap.AddHolding<AioCal>(Constants.AnalogCalBaseName, analogTotal);
                RegisterMap.AddInput<DevUShort>(Constants.InputsRegisterBaseName, inputWords, true);
                RegisterMap.AddInput<DevUShort>(Constants.OutputsRegisterBaseName, outputWords, true);
                RegisterMap.AddHolding<DevUShort>(Constants.CommandedOutputsBaseName, outputWords);
                RegisterMap.AddHolding<DevUShort>("reserved1", 1);
                RegisterMap.AddHolding<DevPumpParams>(Constants.PumpParamsName, 1);
                RegisterMap.AddHolding<DevMotorParams>(Constants.MotorParamsBaseName, pumpsTotal);
                RegisterMap.AddInput<DevMotorReg>(Constants.MotorRegistersBaseName, pumpsTotal, true);
                RegisterMap.AddHolding<DevFloat>(Constants.CommandedSpeedBaseName, pumpsTotal);
            }
            catch (TimeoutException)
            {
                throw; //Allow Connect() to differentiate faults
            }
            catch (Exception ex)
            {
                Log(ex, "Failed to initialize register map.");
                return false;
            }
            return true;
        }

        #endregion

        public Controller(ControllerConfig cfg)
        {
            if (cfg.ConnectionType != ConnectionTypes.Serial) throw new NotImplementedException();
            Config = cfg;
            Port = new SerialPortStream(cfg.PortName);
            PollTimer = new Timer(1000)
            {
                AutoReset = true,
                Enabled = false
            };
            PollTimer.Elapsed += PollTimer_Elapsed;
        }
        public void Dispose()
        {
            Disconnect();
            Master?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region Events

        public event EventHandler<LogEventArgs>? LogEvent;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? UnexpectedDisconnect;
        public event EventHandler<PollResult>? PollCompleted;

        public class DataErrorEventArgs : EventArgs
        {
            public Exception Exception { get; }
            public string Data { get; }
            public DataErrorEventArgs(Exception e, string data)
            {
                Exception = e;
                Data = data;
            }
        }

        public class TerminalEventArgs : EventArgs
        {
            public string Line { get; }
            public TerminalEventArgs(string line)
            {
                Line = line;
            }
        }

        private void Log(Exception? e, string m)
        {
            new Thread(() => { LogEvent?.Invoke(this, new LogEventArgs(e, m)); }).Start();
        }
        private void OnPropertyChanged(string? name = null)
        {
            new Thread(() => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }).Start();
        }
        private void OnUnexpectedDisconnect()
        {
            _IsConnected = false;
            OnPropertyChanged();
            new Thread(() => { UnexpectedDisconnect?.Invoke(this, new EventArgs()); }).Start();
        }

        #endregion

        #region Props

        public static int ConnectionTimeout { get; set; } = 1000; //mS


        public ControllerConfig Config { get; }
        public SerialPortStream Port { get; private set; }
        public bool IsConnected
        {
            get { return _IsConnected; }
            private set
            {
                _IsConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(TotalPumps));
            }
        }
        public Map RegisterMap { get; } = new Map();
        public int TotalPumps => IsConnected ? RegisterMap.GetInputWord(Constants.PumpsNumName) : 0;
        public bool IsPolling => PollTimer.Enabled;

        #endregion

        #region Methods

        public async Task<bool> Connect(string? portName = null)
        {
            if (Port.IsOpen) Port.Close();
            IsConnected = false;
            using (await LockObject.LockAsync())
            {
                if (portName != null) Port.PortName = portName;
                Master?.Dispose(); //Disposes Port as well...
                Port = new SerialPortStream(portName ?? Config.PortName);
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                Adapter = new SerialPortStreamAdapter(Port)
                {
                    ReadTimeout = ConnectionTimeout,
                    WriteTimeout = ConnectionTimeout
                };
                try
                {
                    Port.Open();
                    await Task.Delay(1000);
                    Port.ReadExisting();
                }
                catch (Exception ex)
                {
                    Log(ex, "Failed to open port");
                    return false;
                }
                Master = Factory.CreateRtuMaster(Adapter);
                try
                {
                    IsConnected = await InitRegisterMap();
                    return IsConnected;
                }
                catch (TimeoutException ex)
                {
                    Log(ex, "Connection timeout");
                    Port.Close();
                }
                catch (Exception ex)
                {
                    Log(ex, "Unknown error");
                    Port.Close();
                }
            }
            return false;
        }
        public bool Disconnect()
        {
            IsConnected = false;
            try
            {
                Port.Close();
            }
            catch (Exception e)
            {
                Log(e, "Failed to close the port!");
            }
            return !Port.IsOpen;
        }
        public async Task<IDeviceType> ReadRegister(string name)
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");

            IRegister? reg = RegisterMap.HoldingRegisters[name] as IRegister;
            if (reg != null)
            {
                reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
                return reg.Value;
            }
            reg = RegisterMap.InputRegisters[name] as IRegister;
            if (reg != null)
            {
                reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
                return reg.Value;
            }
            throw new ArgumentException($"Unable to find register '{name}'");
        }
        public async Task<T> ReadRegister<T>(string name) where T : IDeviceType, new()
        {
            return (T)await ReadRegister(name);
        }
        public async Task<T> ReadRegister<T>(Register<T> reg) where T : IDeviceType, new()
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");

            reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
            return reg.TypedValue;
        }
        public async Task<object> ReadRegister(IRegister reg)
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");

            reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
            return reg.Value;
        }
        public async Task WriteRegister(IRegister? reg)
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");
            if (reg == null) throw new ArgumentException("Register can't be null");
            await Master.WriteMultipleRegistersAsync(Config.ModbusAddress, reg.Address, reg.Value.GetWords());
        }
        public async Task WriteRegister(string name)
        {
            var reg = RegisterMap.HoldingRegisters[name] as IRegister;
            await WriteRegister(reg);
        }
        public async Task WriteRegister(string name, IDeviceType value)
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");
            if (RegisterMap.HoldingRegisters[name] is not IRegister reg)
                throw new ArgumentException($"Unknown register: {name}");

            await Master.WriteMultipleRegistersAsync(Config.ModbusAddress, reg.Address, value.GetWords());
        }
        public async Task<PollResult> Poll()
        {
            try
            {
                if (Master == null) throw new Exception("Controller connection does not exist.");
                using (await LockObject.LockAsync())
                {
                    for (int i = 0; i < RegisterMap.PollInputRegisters.Count; i++)
                    {
                        string name = RegisterMap.PollInputRegisters[i];
                        if (RegisterMap.InputRegisters[name] is not IRegister reg)
                            throw new Exception($"Unknown register: {name}");
                        reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
                    }
                    for (int i = 0; i < RegisterMap.PollHoldingRegisters.Count; i++)
                    {
                        string name = RegisterMap.PollHoldingRegisters[i];
                        if (RegisterMap.HoldingRegisters[name] is not IRegister reg)
                            throw new Exception($"Unknown register: {name}");
                        reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
                    }
                }
                var res = new PollResult();
                return res;
            }
            catch (Exception ex)
            {
                Log(ex, "Failed to poll the device");
                throw;
            }
        }
        public async Task ReadAll()
        {
            if (Master == null) throw new Exception("Controller connection does not exist.");
            foreach (DictionaryEntry item in RegisterMap.InputRegisters)
            {
                var reg = (item.Value as IRegister);
                if (reg == null) continue;
                reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
            }
            foreach (DictionaryEntry item in RegisterMap.HoldingRegisters)
            {
                var reg = (item.Value as IRegister);
                if (reg == null) continue;
                reg.Set(await Master.ReadHoldingRegistersAsync(Config.ModbusAddress, reg.Address, reg.Length));
            }
        }
        public void SetAutoPoll(bool enable)
        {
            PollTimer.Enabled = enable;
            OnPropertyChanged(nameof(IsPolling));
        }
        #endregion

    }
}
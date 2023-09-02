using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using ReactorControl.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReactorControl.Providers
{
    public class ScriptProvider : IInputProvider, INotifyPropertyChanged
    {
        public enum ExecutionMode
        {
            Software,
            Hardware
        }
        public enum ExecutionState
        {
            Stopped,
            Paused,
            Running
        }
        public class Command
        {
            public event EventHandler? ActivityChanged;

            public Command()
            {

            }

            protected int _PumpIndex;
            public int PumpIndex 
            {
                get => _PumpIndex;
                set {
                    _PumpIndex = value;
                    PumpIndexString = _PumpIndex.ToString(CultureInfo.InvariantCulture);
                }
            }
            public string? Description {get; set;}
            public float VolumeRate {get; set;}
            public float? TotalVolume {get; set;}
            public float? TotalTime {get; set;}

            [YamlIgnore]
            public double Milliseconds { get; private set; }
            [YamlIgnore]
            public string VolumeRateString { get; private set; } = "";
            [YamlIgnore]
            public string PumpIndexString { get; private set; } = "";
            [YamlIgnore]
            protected bool _IsActive;
            [YamlIgnore]
            public bool IsActive
            {
                get => _IsActive;
                set {
                    _IsActive = value;
                    Dispatcher.UIThread.Post(() => ActivityChanged?.Invoke(this, new EventArgs()));
                }
            }

            public void CalculateTime()
            {
                if (TotalTime != null)
                {
                    Milliseconds = TotalTime.Value * 1000;
                }
                else if (TotalVolume != null)
                {
                    Milliseconds = TotalVolume.Value / VolumeRate * 1000;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Either TotalVolume or TotalVolume must be specified. TotalVolume can't be used with VolumeRate = 0.");
                }
                if (Milliseconds <= 0) throw new InvalidOperationException("Total operation time (calculated) must be greater than 0.");
                VolumeRateString = VolumeRate.ToString(CultureInfo.InvariantCulture);
                PumpIndexString = PumpIndex.ToString(CultureInfo.InvariantCulture);
            }
        }
        public class Thread
        {
            public event EventHandler<Command>? CommandIssued;
            public event EventHandler? Completed;

            public Thread()
            {
                
            }

            public string? Description { get; set; }
            public List<int> Pumps { get; set; } = new List<int>();
            public bool? StopAfterCompletion { get; set; } = true;
            public List<Command> Commands { get; set; } = new List<Command>();

            [YamlIgnore]
            public ExecutionState State { get; private set; } = ExecutionState.Stopped;
            [YamlIgnore]
            public bool IsCompleted { get; private set; } = false;
            [YamlIgnore]
            public Command? ActiveCommand { get; private set; }
            [YamlIgnore]
            public double Progress { get; private set; }

            public void Stop()
            {
                Timer.Stop();
                HrTimer.Reset();
                Index = 0;
                if (ActiveCommand != null)
                {
                    StopCommand.PumpIndex = ActiveCommand.PumpIndex;
                    CommandIssued?.Invoke(this, StopCommand);
                }
                State = ExecutionState.Stopped;
                IsCompleted = false;
            }
            public void Pause()
            {
                if (State != ExecutionState.Running) throw new InvalidOperationException();
                Timer.Stop();
                HrTimer.Stop();
                if (ActiveCommand != null)
                {
                    StopCommand.PumpIndex = ActiveCommand.PumpIndex;
                    CommandIssued?.Invoke(this, StopCommand);
                }
                State = ExecutionState.Paused;
            }
            public void Start()
            {
                if (State != ExecutionState.Stopped) throw new InvalidOperationException();
                State = ExecutionState.Running;
                IsCompleted = false;
                Timer_Elapsed(this, null);
            }
            public void Resume()
            {
                if (State != ExecutionState.Paused) throw new InvalidOperationException();
                if (HrTimer.ElapsedMilliseconds < Timer.Interval) 
                {
                    Timer.Interval -= Timer.Interval - HrTimer.ElapsedMilliseconds;
                    if (ActiveCommand != null)
                    {
                        CommandIssued?.Invoke(this, ActiveCommand);
                    }
                    Timer.Start();
                    HrTimer.Restart();
                }
                else
                {
                    Timer_Elapsed(this, null);
                }
            }
            public void Initialize()
            {
                ProgressIncrement = Commands.Count > 0 ? 1 / Commands.Count : 1;
                Timer.Elapsed += Timer_Elapsed;
                StopCommand.CalculateTime();
                foreach (var item in Commands)
                {
                    item.CalculateTime();
                    if (!Pumps.Contains(item.PumpIndex))
                        throw new InvalidOperationException("Some thread commands refer to undeclared pumps.");
                }
            }

            [YamlIgnore]
            protected Command StopCommand = new Command() { VolumeRate = 0, TotalTime = 1 };
            [YamlIgnore]
            protected Stopwatch HrTimer = new Stopwatch();
            [YamlIgnore]
            protected Timer Timer = new Timer() { AutoReset = false };
            [YamlIgnore]
            protected int Index = 0;
            [YamlIgnore]
            protected double ProgressIncrement;
            private void Timer_Elapsed(object? sender, ElapsedEventArgs? e)
            {
                HrTimer.Reset();
                if (Index < Commands.Count)
                {
                    Command next = Commands[Index++];
                    Timer.Interval = next.Milliseconds;
                    if (ActiveCommand != null) 
                    {
                        ActiveCommand.IsActive = false;
                        if (next.PumpIndex != ActiveCommand.PumpIndex)
                        {
                            StopCommand.PumpIndex = ActiveCommand.PumpIndex;
                            CommandIssued?.Invoke(this, StopCommand);
                        }
                    }
                    Progress += ProgressIncrement;
                    CommandIssued?.Invoke(this, next);
                    Timer.Start();
                    HrTimer.Start();
                    ActiveCommand = next;
                    next.IsActive = true;
                }
                else
                {
                    Progress = 1;
                    if (ActiveCommand != null) 
                    {
                        ActiveCommand.IsActive = false;
                        if (StopAfterCompletion ?? true)
                        {
                            StopCommand.PumpIndex = ActiveCommand.PumpIndex;
                            CommandIssued?.Invoke(this, StopCommand);
                        }
                    }
                    ActiveCommand = null;
                    State = ExecutionState.Stopped;
                    IsCompleted = true;
                    Completed?.Invoke(this, new EventArgs());
                }
            }
        }
        public class Script
        {
            public Script()
            {

            }

            public string Name {get; set;} = "Example";
            public ExecutionMode Mode {get; set;} = ExecutionMode.Software;
            public List<Thread> Threads {get; set;} = new List<Thread>();

            [YamlIgnore]
            public List<int> PumpsUsed { get; private set; } = new List<int>();
            [YamlIgnore]
            public string TargetDeviceName {get; set;} = "Example";

            public void Validate()
            {
                foreach (var thread in Threads)
                {
                    PumpsUsed.AddRange(thread.Pumps);
                }
                if (PumpsUsed.Distinct().Count() < PumpsUsed.Count)
                    throw new InvalidOperationException("Conflicting thread declarations: a pump can't belong to more than one thread.");
                foreach (var thread in Threads)
                {
                    thread.Initialize();
                }
            }
        }

        public static async Task<Script> ReadScript(string filePath)
        {
            Script? instance;
            try
            {
                instance = IInputProvider.DeserializerInstance.Deserialize<Script>(await File.ReadAllTextAsync(filePath));
            }
            catch (Exception ex)
            {
                throw new AggregateException("Failed to read or deserialize script file.", ex);
            }
            try
            {
                if (instance == null) throw new NullReferenceException("Deserializer returned NULL");
                instance.Validate();
                return instance;
            }
            catch (InvalidOperationException ex)
            {
                throw new AggregateException("Invalid script file.", ex);
            }
            catch (Exception ex)
            {
                throw new AggregateException("Unknown error during script validation.", ex);
            }
        }
        public static Script ExampleScript { get; } = new Script()
        {
            Threads = new List<Thread>()
            {
                new Thread()
                {
                    Description = "Example Thread 1",
                    Pumps = new List<int>() { 0, 1 },
                    Commands = new List<Command> {
                        new Command() {
                            Description = "Run for 10 seconds @ 0.15mL/s",
                            PumpIndex = 0,
                            TotalTime = 10,
                            VolumeRate = 0.15f
                        },
                        new Command() {
                            Description = "Pump 1.5mL @ 15mL/s",
                            PumpIndex = 1,
                            VolumeRate = 0.15f,
                            TotalVolume = 1.5f
                        }
                    }
                },
                new Thread()
                {
                    Description = "Example Thread 2",
                    Pumps = new List<int>() { 2 },
                    Commands = new List<Command> {
                        new Command() {
                            Description = "Run for 10 seconds @ 0.15mL/s",
                            PumpIndex = 2,
                            TotalTime = 10,
                            VolumeRate = 0.15f
                        },
                        new Command() {
                            Description = "Pump 1.5mL @ 15mL/s",
                            PumpIndex = 2,
                            VolumeRate = 0.15f,
                            TotalVolume = 1.5f
                        }
                    }
                }
            }
        };
        public async static Task WriteExampleScript(string filePath)
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            await File.WriteAllTextAsync(filePath, s.Serialize(ExampleScript));
        }

        public event EventHandler<LogEventArgs>? LogEvent;
        public event EventHandler<IpcEventArgs>? CommandReceived;
        public event EventHandler? ClientConnected;
        public event EventHandler? ClientDisconnected;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ScriptProvider(Script s)
        {
            ScriptInstance = s;
            foreach (var thread in ScriptInstance.Threads)
            {
                thread.CommandIssued += Thread_CommandIssued;
                thread.Completed += Thread_Completed;
            }
        }

        public Script ScriptInstance { get; }
        public ExecutionState State { get; private set; } = ExecutionState.Stopped;
        public bool IsCompleted { get; private set; } = false;
        public bool IsAborted { get; private set; } = false;
        public double Progress { get; private set; }
        public int TotalThreads => ScriptInstance.Threads.Count;

        public void Start()
        {
            if (State != ExecutionState.Stopped) throw new InvalidOperationException();
            IsCompleted = false;
            IsAborted = false;
            foreach (var thread in ScriptInstance.Threads)
            {
                thread.Start();
            }
            State = ExecutionState.Running;
            Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAborted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }
        public void Stop()
        {
            foreach (var thread in ScriptInstance.Threads)
            {
                thread.Stop();
            }
            if (!IsCompleted) IsAborted = true;
            IsCompleted = false;
            State = ExecutionState.Stopped;
            Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAborted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }
        public void Resume()
        {
            if (State != ExecutionState.Paused) throw new InvalidOperationException();
            foreach (var thread in ScriptInstance.Threads)
            {
                if (thread.State == ExecutionState.Paused) thread.Resume();
            }
            State = ExecutionState.Running;
            Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }
        public void Pause()
        {
            if (State != ExecutionState.Running) throw new InvalidOperationException();
            foreach (var thread in ScriptInstance.Threads)
            {
                if (thread.State == ExecutionState.Running) thread.Pause();
            }
            State = ExecutionState.Paused;
            Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            });
        }

        protected object CompletionLock = new();
        protected void Thread_CommandIssued(object? sender, Command e)
        {
            IpcEventArgs ipc = new()
            {
                SourceTag = "Script",
                RegisterName = Constants.CommandedSpeedBaseName + e.PumpIndexString,
                TargetDeviceName = ScriptInstance.TargetDeviceName,
                ValueString = e.VolumeRateString
            };
            lock (this)
            {
                CommandReceived?.Invoke(this, ipc);
            }
            Dispatcher.UIThread.Post(() =>
            {
                Progress = ScriptInstance.Threads.Min(x => x.Progress);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            });
        }
        protected void Thread_Completed(object? sender, EventArgs e)
        {
            if (IsCompleted) return;
            bool all = false;
            lock (CompletionLock) 
            {
                all = ScriptInstance.Threads.All(x => x.IsCompleted);
                if (all)
                {
                    IsCompleted = true;
                    IsAborted = false;
                    State = ExecutionState.Stopped;
                }
            }
            if (all)
            {
                Dispatcher.UIThread.Post(() => {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAborted)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                });
            }
            Dispatcher.UIThread.Post(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))));
        }
    }
}

using System;

namespace ReactorControl
{
    public class LogEventArgs : EventArgs
    {
        public Exception? Exception { get; }
        public string Message { get; }
        public LogEventArgs(Exception? e, string msg)
        {
            Exception = e;
            Message = msg;
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using ModbusRegisterMap;
using ReactorControl.Models;
using ReactorControl.Providers;

namespace ReactorControl.ViewModels
{
    public class ScriptThreadViewModel : ViewModelBase
    {
        protected ScriptProvider.Thread mThread;

        public ScriptThreadViewModel(ScriptProvider.Thread t)
        {
            mThread = t;
            foreach (var item in mThread.Commands)
            {
                Commands.Add(new ScriptCommandViewModel(item));
            }
        }

        public ObservableCollection<ScriptCommandViewModel> Commands { get; } = new ObservableCollection<ScriptCommandViewModel>();
        public string ColumnHeader => mThread.Description ?? $"Pumps: {string.Join(',', mThread.Pumps.Select(x => x.ToString()))}";
    }
}
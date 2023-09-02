using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        public string ColumnHeader => 
            $"{mThread.Description}{Environment.NewLine}Pumps: {string.Join(',', mThread.Pumps.Select(x => x.ToString()))}";
    }
}
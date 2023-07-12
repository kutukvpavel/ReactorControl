using Avalonia.Media;
using ModbusRegisterMap;
using ReactorControl.Models;
using System;
using System.Globalization;

namespace ReactorControl.ViewModels
{
    public class ProbeControlViewModel : ViewModelBase
    {
        protected IRegister? mRegister;
        protected ProbeConfig mConfig;
        protected Controller mController;

        protected void TryGetRegister()
        {
            if (mRegister != null) mRegister.PropertyChanged -= MRegister_PropertyChanged;
            mRegister = mController.RegisterMap.InputRegisters[mConfig.RegisterName] as IRegister;
            if (mRegister == null) return;
            mRegister.PropertyChanged += MRegister_PropertyChanged;
            MRegister_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(null));
        }

        public ProbeControlViewModel(Controller c, ProbeConfig cfg)
        {
            mController = c;
            mConfig = cfg;
            mController.PropertyChanged += MController_PropertyChanged;
            TryGetRegister();
        }

        private void MController_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != mConfig.TriggerPropertyName) return;
            TryGetRegister();
        }

        private void MRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ValueString));
            RaisePropertyChanged(nameof(StatusString));
            RaisePropertyChanged(nameof(StatusColor));
        }

        public string ValueString
        {
            get
            {
                if (mRegister == null) return string.Empty;
                return mConfig.DisplayFormat == null ?
                    mRegister.Value.ToString() ?? string.Empty :
                    mRegister.Value.ToString(mConfig.DisplayFormat, CultureInfo.CurrentUICulture);
            }
        }
        public string Units => mConfig.Units;
        public string Name => mConfig.DisplayName;
        public string StatusString => mConfig.GetStatusString(mRegister);
        public IBrush StatusColor => mConfig.GetStatusColor(mRegister);

        public void UpdateSettingsContext()
        {
            TryGetRegister();
            RaisePropertyChanged(nameof(Units));
            RaisePropertyChanged(nameof(Name));
        }
    }
}

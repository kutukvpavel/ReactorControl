using System;
using System.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReactorControl
{
    public class Settings
    {
        protected const string DeviceCategory = "Device";
        protected const string StorageCategory = "Storage";
        protected const string ViewCategory = "View";

        public static Settings Deserialize(string yaml)
        {
            var d = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            return d.Deserialize<Settings>(yaml);
        }

        public Settings()
        {

        }

        [Category(DeviceCategory)]
        [DisplayName("Modbus request timeout")]
        [DefaultValue(500)]
        public int ConnectionTimeout {get;set;} = 500; //mS
        [Category(DeviceCategory)]
        [DisplayName("Data poll interval")]
        [DefaultValue(500)]
        public int PollInterval {get;set;} = 500; //mS
        [Category(DeviceCategory)]
        [DisplayName("Keep-alive interval")]
        [DefaultValue(1000)]
        public int KeepAliveInterval {get;set;} = 1000; //mS

        [Category(StorageCategory)]
        [DisplayName("Device conf. folder path")]
        [DefaultValue("devices")]
        public string DeviceFolder { get; set; } = "devices";
        [Category(StorageCategory)]
        [DisplayName("Device conf. backup folder path")]
        [DefaultValue("backup")]
        public string DeviceBackupFolder { get; set; } = "backup";
        [Category(StorageCategory)]
        [DisplayName("Maximum number of backup files")]
        [DefaultValue(10)]
        public int MaxDeviceBackupsToStore { get; set; } = 10;
        [Category(StorageCategory)]
        [DisplayName("Ignore device conf. backup errors")]
        [DefaultValue(false)]
        public bool IgnoreDeviceFolderBackupErrors { get; set; } = false;

        [Category(ViewCategory)]
        [DisplayName("Main window width (px)")]
        [DefaultValue(800)]
        public double MainWindowWidth {get;set;} = 800;
        [Category(ViewCategory)]
        [DisplayName("Main window height (px)")]
        [DefaultValue(600)]
        public double MainWindowHeight {get;set;} = 600;

        public string Serialize()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            return s.Serialize(this);
        }
    }
}

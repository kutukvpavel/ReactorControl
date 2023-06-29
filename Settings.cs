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

        [Category(StorageCategory)]
        [DisplayName("Device conf. folder path")]
        public string DeviceFolder { get; set; } = "devices";
        [Category(StorageCategory)]
        [DisplayName("Device conf. backup folder path")]
        public string DeviceBackupFolder { get; set; } = "backup";
        [Category(StorageCategory)]
        [DisplayName("Maximum number of backup files")]
        public int MaxDeviceBackupsToStore { get; set; } = 10;
        [Category(StorageCategory)]
        [DisplayName("Ignore device conf. backup errors")]
        public bool IgnoreDeviceFolderBackupErrors { get; set; } = false;

        public string Serialize()
        {
            var s = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            return s.Serialize(this);
        }
    }
}

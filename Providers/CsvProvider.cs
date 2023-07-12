using System;
using System.IO;

namespace ReactorControl.Providers
{
    public class CsvProvider : OutputProviderBase
    {
        public static string Separator { get; set; } = ",";

        protected TextWriter? mTextWriter;

        public CsvProvider(string folderPath, string tag) : base(tag)
        {
            FolderPath = folderPath;
        }

        public string FolderPath { get; }

        protected override void Send(string data)
        {
            if (mTextWriter == null) throw new NullReferenceException("Csv writer not initialized");
            mTextWriter.WriteLine(data);
        }
        protected override string ConvertData(Data d)
        {
            return $"{d.Timestamp}{Separator}{d.RegisterName}{Separator}{d.ValueString}";
        }

        public override void Connect()
        {
            if (IsConnected) Disconnect();
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                string path = Path.Combine(Path.GetFullPath(FolderPath), $"{Tag}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
                mTextWriter = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                base.Connect();
            }
            catch (Exception ex)
            {
                Log("Failed to create CSV output file", ex);
            }
        }
        public override void Disconnect()
        {
            if (!IsConnected) return;
            try
            {
                mTextWriter?.Close();
                mTextWriter?.Dispose();
            }
            catch (Exception ex)
            {
                Log("Failed to close CSV output file", ex);
            }
            base.Disconnect();
        }
    }
}

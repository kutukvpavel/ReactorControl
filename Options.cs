using System;
using CommandLine;

namespace ReactorControl
{
#nullable disable
    public class Options
    {
        [Option('s', "settings", Default = "settings.yaml", HelpText = "Absolute or relative (to working directory) path to settings file (YAML)")]
        public string SettingFilePath { get; set; }
    }
#nullable restore
}

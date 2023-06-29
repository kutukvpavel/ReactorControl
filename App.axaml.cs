using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommandLine;
using LLibrary;
using ReactorControl.Models;
using ReactorControl.ViewModels;
using ReactorControl.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ReactorControl;

public partial class App : Application
{
    public const string DeviceFileExtension = ".yaml";
    public const string DeviceFileSearchPattern = "*" + DeviceFileExtension;
    public Window? MainWindow { get; private set; }
    public L Logger { get; } = new L();
    public Settings Settings { get; private set; } = new Settings();
    public Options CliOptions { get; private set; } = new Options();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public ObservableCollection<Controller> Controllers { get; set; } = new ObservableCollection<Controller>();

    public void LogError(string message, Exception ex)
    {
        Logger.Error($"{message}: {ex}");
    }
    public void Log(string message)
    {
        Logger.Info(message);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Parser.Default.ParseArguments<Options>(desktop.Args).WithParsed((o) =>
            {
                CliOptions = o;
            });
            LoadSettings();
            LoadDevices();

            var vm = new MainWindowViewModel(Controllers, Settings);
            vm.LoadRequest += Vm_LoadRequest;
            vm.SaveRequest += Vm_SaveRequest;
            vm.LogDataReceived += Vm_LogDataReceived;
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Vm_LogDataReceived(object? sender, LogEventArgs e)
    {
        string msg = $"{sender} :: {e.Message}";
        if (e.Exception == null)
        {
            Log(msg);
        }
        else
        {
            LogError(msg, e.Exception);
        }
    }

    private void Vm_SaveRequest(object? sender, EventArgs e)
    {
        SaveDevices();
    }
    private void Vm_LoadRequest(object? sender, EventArgs e)
    {
        LoadDevices();
    }
    protected static string GetFullyQualifiedPath(string orig)
    {
        if (Path.IsPathFullyQualified(orig))
            return orig;
        return Path.GetFullPath(orig, Environment.CurrentDirectory);
    }
    protected static void EnsureDirectoryExists(string p)
    {
        string? d;
        if (!Path.HasExtension(p))
        {
            d = p;
        }
        else
        {
            d = Path.GetDirectoryName(p);
        }
        if (d == null) throw new ArgumentException($"Something is wrong with this path: {p}");
        if (!Directory.Exists(d))
        {
            Directory.CreateDirectory(d);
        }
    }
    protected string GetSettingsPath()
    {
        return GetFullyQualifiedPath(CliOptions.SettingFilePath);
    }
    protected string GetDevicesPath()
    {
        return GetFullyQualifiedPath(Settings.DeviceFolder);
    }

    protected void LoadSettings()
    {
        try
        {
            string p = GetSettingsPath();
            EnsureDirectoryExists(p);
            if (!File.Exists(p))
            {
                Log($"No settings file found at '{p}', creating default one...");
                SaveSettings();
            }
            Settings = Settings.Deserialize(File.ReadAllText(p));
            Log($"Settings file loaded from '{p}'");
        }
        catch (Exception ex)
        {
            LogError("Failed to load settings file", ex);
        }
    }
    protected void SaveSettings()
    {
        try
        {
            string p = GetSettingsPath();
            EnsureDirectoryExists(p);
            File.WriteAllText(p, Settings.Serialize());
            Log($"Setting file saved to '{p}'");
        }
        catch (Exception ex)
        {
            LogError("Failed to save settings file", ex);
        }
    }

    protected void LoadDevices()
    {
        string dir;
        Log("Device load started...");
        try
        {
            dir = GetDevicesPath();
            EnsureDirectoryExists(dir);
        }
        catch (Exception ex)
        {
            LogError("Failed to derive device folder path", ex);
            return;
        }
        foreach (var item in Directory.EnumerateFiles(dir, DeviceFileSearchPattern, SearchOption.AllDirectories))
        {
            try
            {
                var c = new Controller(ControllerConfig.Deserialize(File.ReadAllText(item)));
                Dispatcher.UIThread.Invoke(() =>
                {
                    Controllers.Add(c);
                });
                Log($"Loaded device '{c.Config.Name}' from '{item}'");
            }
            catch (Exception ex)
            {
                LogError($"Failed to read device configuration from '{item}'", ex);
            }
        }
        Log("Device load finished.");
    }
    protected void SaveDevices()
    {
        string dir;
        Log("Device save started...");
        try
        {
            dir = GetDevicesPath();
            EnsureDirectoryExists(dir);
        }
        catch (Exception ex)
        {
            LogError("Failed to derive device folder path", ex);
            return;
        }
        try
        {
            if (Directory.GetFiles(dir).Length > 0)
            {
                string backup = GetFullyQualifiedPath(Settings.DeviceBackupFolder);
                EnsureDirectoryExists(backup);
                ZipFile.CreateFromDirectory(dir, Path.Combine(backup, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip"));
                string[] files = Directory.GetFiles(backup, "*.zip", SearchOption.TopDirectoryOnly);
                if (files.Length > Settings.MaxDeviceBackupsToStore)
                {
                    File.Delete(files.OrderBy(x => x).First());
                }
            }
        }
        catch (Exception ex)
        {
            LogError("Failed to make device folder backup", ex);
            if (!Settings.IgnoreDeviceFolderBackupErrors) return;
            Log($"{nameof(Settings.IgnoreDeviceFolderBackupErrors)} is set to true, continuing device save...");
        }
        foreach (var item in Directory.GetFiles(dir, DeviceFileSearchPattern, SearchOption.AllDirectories))
        {
            try
            {
                File.Delete(item);
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete old device file '{item}'", ex);
            }
        }
        foreach (var item in Controllers)
        {
            string p = string.Empty;
            try
            {
                p = Path.Combine(dir, item.Config.Name + DeviceFileExtension);
                File.WriteAllText(p, item.Config.Serialize());
                Log($"Device '{item.Config.Name}' saved at '{p}'");
            }
            catch (Exception ex)
            {
                LogError($"Failed to save device file '{p}'", ex);
            }
        }
        Log("Device save finished.");
    }
}
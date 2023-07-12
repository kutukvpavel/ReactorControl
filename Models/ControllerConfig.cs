using Avalonia.Media;
using ModbusRegisterMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReactorControl.Models;

public enum ConnectionTypes
{
    Serial,
    TCP
}

public class ControllerConfig
{
    public static ControllerConfig Deserialize(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<ControllerConfig>(yaml);
    }

    public ControllerConfig()
    {

    }
    public ControllerConfig(List<ProbeConfig> initialProbeCfg)
    {
        Probes = initialProbeCfg.ToArray();
    }

    [DisplayName("Name to be displayed")]
    [DefaultValue("Example")]
    public string Name {get;set;} = "Example";
    [DisplayName("MODBUS type (serial/TCP)")]
    [DefaultValue(ConnectionTypes.Serial)]
    public ConnectionTypes ConnectionType {get;set;} = ConnectionTypes.Serial;
    [DisplayName("Serial port name")]
    [DefaultValue("COM1")]
    public string PortName {get;set;} = "COM1";
    [DisplayName("IP address for MODBUS TCP")]
    [DefaultValue("127.0.0.1")]
    public string IPAddress { get; set; } = "127.0.0.1";
    [DisplayName("MODBUS station address")]
    [DefaultValue(1)]
    public byte ModbusAddress {get;set;} = 1;
    [DisplayName("Volume rate units to display")]
    [DefaultValue("mL/min")]
    public string VolumeRateUnit { get; set; } = "mL/min";
    [DisplayName("Probes to display (right column)")]
    public ProbeConfig[] Probes { get; set; } = Array.Empty<ProbeConfig>();
    [DisplayName("IPC socket port (-1 to disable)")]
    [DefaultValue(-1)]
    public int IpcSocketPort { get; set; } = -1;
    [DisplayName("IPC pipe name (empty to disable)")]
    [DefaultValue("")]
    public string IpcPipeName { get; set; } = string.Empty;

    public string Serialize()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(this);
    }
}
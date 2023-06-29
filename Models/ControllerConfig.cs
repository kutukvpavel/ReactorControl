using System;
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

    public string Name {get;set;} = "Example";
    public ConnectionTypes ConnectionType {get;set;} = ConnectionTypes.Serial;
    public string PortName {get;set;} = "COM1";
    public string IPAddress { get; set; } = "127.0.0.1";
    public byte ModbusAddress {get;set;} = 1;
    public string VolumeRateUnit { get; set; } = "mL/min";

    public string Serialize()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(this);
    }
}
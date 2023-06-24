using System;
using System.Security;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReactorControl.Models;

public class ControllerConfig
{
    public enum ConnectionTypes
    {
        Serial,
        TCP
    }

    public ControllerConfig()
    {

    }

    public ConnectionTypes ConnectionType {get;set;} = ConnectionTypes.Serial;
    public string PortName {get;set;} = "COM1";
    public int ModbusAddress {get;set;} = 1;

    public string Serialize()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(this);
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Model;

public sealed class Measurement
{
    [Obsolete("Deserialization only")]
    public Measurement()
    {
    }

    public Measurement(string type, string addr, int measurement)
    {
        MacAddress = addr;
        Type = type;
        Value = measurement;
    }

    public string MacAddress { get; set; }

    public string Type { get; set; }

    public int Value { get; set; }
}
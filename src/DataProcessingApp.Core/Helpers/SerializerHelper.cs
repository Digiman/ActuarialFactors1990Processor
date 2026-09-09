using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DataProcessingApp.Core.Helpers;

public static class SerializerHelper
{
    // the 90CM series JSON stores ages and factors as strings and uses
    // camelCase keys, the root JSON files use PascalCase property names
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static string Serialize<T>(T data, SerializeFormat format)
    {
        return format switch
        {
            SerializeFormat.XML => SerializeToXml(data),
            SerializeFormat.JSON => SerializeToJson(data),
            _ => String.Empty
        };
    }

    public static T Deserialize<T>(string data, SerializeFormat format)
    {
        return format switch
        {
            SerializeFormat.XML => DeserializeToXml<T>(data),
            SerializeFormat.JSON => DeserializeFromJson<T>(data),
            _ => default
        };
    }

    private static string SerializeToXml<T>(T data)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var stringWriter = new StringWriter())
        {
            xmlSerializer.Serialize(stringWriter, data);
            return stringWriter.ToString();
        }
    }

    private static T DeserializeToXml<T>(string data)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        var stream = GenerateStreamFromString(data);
        var result = xmlSerializer.Deserialize(stream);
        return (T)result;
    }

    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string SerializeToJson<T>(T data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    private static T DeserializeFromJson<T>(string data)
    {
        return JsonSerializer.Deserialize<T>(data, JsonOptions);
    }
}

public enum SerializeFormat
{
    XML = 1,
    JSON = 2
}
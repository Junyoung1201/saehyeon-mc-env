using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class JsonUtil
{
    public static string Stringify(object obj, bool indented = false)
    {
        return JsonConvert.SerializeObject(
            obj,
            indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None
        );
    }

    public static T Parse<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 문자열이 비어있습니다.", nameof(json));

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static dynamic Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 문자열이 비어있습니다.", nameof(json));

        return JsonConvert.DeserializeObject<dynamic>(json);
    }

    public static JToken ParseJToken(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 문자열이 비어있습니다.", nameof(json));

        return JToken.Parse(json);
    }

    public static T ReadFromFile<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("JSON 파일을 찾을 수 없습니다.", path);

        string json = File.ReadAllText(path);
        return Parse<T>(json);
    }

    public static void WriteToFile(object obj, string path, bool indented = false)
    {
        string json = Stringify(obj, indented);
        File.WriteAllText(path, json);
    }
}

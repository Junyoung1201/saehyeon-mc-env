using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class JSON
{
    public static Task<dynamic> Parse(string json)
    {
        return Task.Run(() => JsonConvert.DeserializeObject<dynamic>(json));
    }

    public static Task<string> Stringify(object obj)
    {
        return Task.Run(() => JsonConvert.SerializeObject(obj));
    }

    public static void Values(JObject obj)
    {
        foreach (KeyValuePair<string, JToken> kv in obj)
        {
            Console.WriteLine($"키: {kv.Key}, 값: {kv.Value}");
        }
    }

    public static void Set(JObject src, string value, dynamic newData)
    {
        src[value] = JToken.FromObject(newData);
    }

    public static async Task<dynamic> ReadFile(string path)
    {
        using (var reader = new StreamReader(path))
        {
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            return await Parse(content).ConfigureAwait(false);
        }
    }

    public static async Task WriteFile(string path, object obj)
    {
        var content = await Stringify(obj).ConfigureAwait(false);
        using (var writer = new StreamWriter(path, false))
        {
            await writer.WriteAsync(content).ConfigureAwait(false);
        }
    }

    public static dynamic ReadFileSync(string path)
    {
        var content = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<dynamic>(content);
    }

    public static void WriteFileSync(string path, object obj)
    {
        var content = JsonConvert.SerializeObject(obj);
        File.WriteAllText(path, content);
    }
}

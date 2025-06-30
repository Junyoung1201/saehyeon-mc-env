using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class JsonUtils
{
    public static string Serialize(object obj, Formatting formatting = Formatting.None)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return JsonConvert.SerializeObject(obj, formatting);
    }

    public static T Deserialize<T>(string json)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static JToken Deserializ(string json)
    {
        return Deserialize<JToken>(json);
    }

    public static async Task<T> ReadFile<T>(string path, CancellationToken ct = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("경로가 비어 있습니다.", nameof(path));
        ct.ThrowIfCancellationRequested();

        using (var reader = new StreamReader(path))
        {
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            return Deserialize<T>(content);
        }
    }

    public static Task<JToken> ReadFile(string path, CancellationToken ct = default(CancellationToken))
    {
        return ReadFile<JToken>(path, ct);
    }

    public static async Task WriteFile<T>(string path, T obj, bool overwrite = true, CancellationToken ct = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("경로가 비어 있습니다.", nameof(path));
        ct.ThrowIfCancellationRequested();

        var content = Serialize(obj, Formatting.Indented);
        var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

        using (var fs = new FileStream(path, fileMode, FileAccess.Write, FileShare.None))
        {
            using (var writer = new StreamWriter(fs))
            {
                await writer.WriteAsync(content).ConfigureAwait(false);
            }
        }
    }

    public static T ReadFileSync<T>(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("경로가 비어 있습니다.", nameof(path));
        var content = File.ReadAllText(path);
        return Deserialize<T>(content);
    }

    public static JToken ReadFileSync(string path)
    {
        return ReadFileSync<JToken>(path);
    }

    public static void WriteFileSync<T>(string path, T obj, bool overwrite = true)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("경로가 비어 있습니다.", nameof(path));
        var content = Serialize(obj, Formatting.Indented);
        var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

        using (var fs = new FileStream(path, fileMode, FileAccess.Write, FileShare.None))
        {
            using (var writer = new StreamWriter(fs))
            {
                writer.Write(content);
            }
        }
    }

    public static IEnumerable<KeyValuePair<string, JToken>> EnumerateProperties(JObject obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return obj;
    }

    public static void SetPropertySync(JObject obj, string name, object value)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("프로퍼티 이름이 비어 있습니다.", nameof(name));
        obj[name] = value is JToken ? (JToken)value : JToken.FromObject(value);
    }
}

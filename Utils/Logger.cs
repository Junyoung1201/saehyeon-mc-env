using System;
using System.IO;

public static class Logger
{
    private static string _logFilePath;
    private static StreamWriter _writer;
    private static readonly object _lock = new object();
    private static bool _handlersRegistered = false;
    private static bool _prefix = true;
    private static bool _outputPrefix = false;
    private static bool _output = true;

    public static void SetOutput(bool output)
    {
        _output = output;
    }

    public static void SetPrefixOutput(bool outputPrefix)
    {
        _outputPrefix = outputPrefix;
    }

    public static void SetWritePrefix(bool prefix)
    {
        Logger._prefix = prefix;
    }

    public static void StartWriteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("경로가 비어있습니다.", nameof(path));

        if (Path.GetExtension(path).Equals(".log"))
        {
            _logFilePath = path;

            // 이미 같은 이름의 파일이 있으면 제거
            if(File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }
        else
        {
            // 폴더 준비
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // 파일명 생성
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".log";
            _logFilePath = Path.Combine(path, fileName);
        }

        // 스트림 열기
        _writer = new StreamWriter(_logFilePath, append: true)
        {
            AutoFlush = true
        };

        // 종료 이벤트 등록
        if (!_handlersRegistered)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
            Console.CancelKeyPress += (s, e) =>
            {
                Close();
                e.Cancel = false;
            };
            _handlersRegistered = true;
        }
    }

    public static void Log(string message, string prefix = "INFO", bool output = true)
    {
        string timeTag = DateTime.Now.ToString("yyyy-MM-dd");
        string prefixStr = _prefix ? $"[{timeTag}] [{prefix}] " : "";
        string logLine = $"{prefixStr}{message}";

        // 콘솔 출력
        if(_output && output)
        {
            string outputLine;

            if (_outputPrefix)
            {
                outputLine = prefix + message;
            }
            else
            {
                outputLine = message;
            }
            Console.WriteLine(outputLine);
        }

        // 파일 출력
        if (_writer != null)
        {
            lock (_lock)
            {
                _writer.WriteLine(logLine);
            }
        }
    }

    public static void Close()
    {
        lock (_lock)
        {
            _writer?.Close();
            _writer = null;
        }
    }

    public static void Info(string message)
    {
        Logger.Log(message, "INFO");
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Logger.Log(message, "WARN");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Logger.Log(message, "ERROR");
        Console.ResetColor();
    }
}

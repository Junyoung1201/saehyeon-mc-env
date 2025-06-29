using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

public static class ShellUtil
{
    public class Result
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
    }

    public static Result Run(string command, string arguments = "", int timeoutMilliseconds = 60000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using (var process = new Process { StartInfo = psi })
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(timeoutMilliseconds))
            {
                process.Kill();
                throw new TimeoutException($"프로세스가 {timeoutMilliseconds}ms 내에 종료되지 않았습니다.");
            }

            return new Result
            {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString()
            };
        }
    }

    public static async Task<Result> RunAsync(
        string command,
        string arguments = "",
        int timeoutMilliseconds = 60000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            var exitTcs = new TaskCompletionSource<bool>();

            process.Exited += (s, e) =>
            {
                exitTcs.TrySetResult(true);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 프로세스 종료 또는 타임아웃 대기
            var completed = await Task.WhenAny(
                exitTcs.Task,
                Task.Delay(timeoutMilliseconds)
            );

            if (completed != exitTcs.Task)
            {
                try { process.Kill(); } catch { /* 무시 */ }
                throw new TimeoutException($"프로세스가 {timeoutMilliseconds}ms 내에 종료되지 않았습니다.");
            }

            // 안전하게 이벤트 핸들러가 모두 처리되도록 잠시 대기
            await exitTcs.Task;

            return new Result
            {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString()
            };
        }
    }
}
using System.Diagnostics;
using System.Text;

namespace Zomlib;

public static class ProcessLauncher
{
    public static ProcessExecution Start(string exe, IEnumerable<string> args, ILogger? logger, CancellationToken token = default)
    {
        logger?.LogInformation($"Starting {exe} {string.Join(' ', args.Select(arg => arg.Contains(' ', StringComparison.Ordinal) ? $"\"{arg}\"" : arg))}");

        var info = new ProcessStartInfo(exe)
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var arg in args)
            info.ArgumentList.Add(arg);

        var process = Process.Start(info) ?? throw new InvalidOperationException($"Could not start {exe}");
        token.UnsafeRegister(_ => process.Close(), null);

        return new ProcessExecution(process, logger, token);
    }


    public class ProcessExecution : IDisposable
    {
        public StreamReader StdOut => Process.StandardOutput;
        public StreamReader StdErr => Process.StandardError;
        readonly Process Process;
        readonly ILogger? Logger;
        readonly CancellationToken Token;

        public ProcessExecution(Process process, ILogger? logger, CancellationToken token)
        {
            Process = process;
            Logger = logger;
            Token = token;
        }

        public async Task<string> WaitForExit(bool ensureSuccess, LogLevel stdout = LogLevel.Information, LogLevel stderr = LogLevel.Error)
        {
            var sbuilder = new StringBuilder();
            await WaitForExit(ensureSuccess, (_, line) => sbuilder.AppendLine(line), stdout, stderr);

            return sbuilder.ToString();
        }
        public async Task WaitForExit(bool ensureSuccess, Action<bool, string>? onread, LogLevel stdout = LogLevel.Information, LogLevel stderr = LogLevel.Error)
        {
            var reading = StartReadingOutput(Process, onread, Logger, stdout, stderr);

            await Process.WaitForExitAsync(Token);
            await reading;

            if (ensureSuccess && Process.ExitCode != 0)
                throw new Exception($"Task process ended with exit code {Process.ExitCode}");
        }

        static Task StartReadingOutput(Process process, Action<bool, string>? onread, ILogger? logger, LogLevel stdout, LogLevel stderr)
        {
            return Task.WhenAll(
                startReading(process.StandardOutput, false),
                startReading(process.StandardError, true)
            );


            async Task startReading(StreamReader input, bool err)
            {
                while (true)
                {
                    var str = await input.ReadLineAsync().ConfigureAwait(false);
                    if (str is null) return;

                    var logstr = $"[Process {process.Id}] {str}";
                    if (err) logger?.Log(stderr, logstr);
                    else logger?.Log(stdout, logstr);

                    onread?.Invoke(err, str);
                }
            }
        }


        public void Dispose()
        {
            Process.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

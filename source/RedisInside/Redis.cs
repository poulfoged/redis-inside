using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using RedisInside.Executables;

namespace RedisInside
{
    /// <summary>
    /// Run integration-tests against Redis
    /// </summary>
    public class Redis : IDisposable
    {
        private bool _disposed;
        private readonly Process process;
        private readonly TemporaryFile executable;
        private readonly Config config = new Config();

        public Redis(Action<IConfig> configuration = null)
        {
            configuration?.Invoke(config);

            string arguments = $"--port {config.port} --bind 127.0.0.1";

            if (config.useExternalBinary)
            {
                var finder = IsLinux() ? "which" : "where";

                var sp = Process.Start(new ProcessStartInfo(finder)
                {
                    UseShellExecute = false,
                    Arguments = "redis-server",
                    WindowStyle = ProcessWindowStyle.Maximized,
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.ASCII,
                });

                if (sp == null)
                {
                    throw new Exception("Could not start 'which redis-server' properly.");
                }

                sp.Start();

                var externalPath = sp.StandardOutput.ReadToEnd().Trim();

                sp.WaitForExit();

                if (string.IsNullOrEmpty(externalPath))
                {
                    throw new Exception("Could not locate redis-server binary.");
                }

                if (!File.Exists(externalPath))
                {
                    throw new Exception($"Found invalid redis-server path (file does not exists or unreadable): {externalPath}");
                }

                executable = new TemporaryFile(File.OpenRead(externalPath));
            }
            else
            {
                executable = new TemporaryFile(typeof(RessourceTarget).Assembly.GetManifestResourceStream(typeof(RessourceTarget), "redis-server.exe"), "exe");
                arguments += " --persistence-available no";
            }

            var processStartInfo = new ProcessStartInfo(" \"" + executable.Info.FullName + " \"")
            {
                UseShellExecute = false,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Maximized,
                CreateNoWindow = true,
                LoadUserProfile = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
            };

            process = Process.Start(processStartInfo);
            process.ErrorDataReceived += (sender, eventargs) => config.logger.Invoke(eventargs.Data);
            process.OutputDataReceived += (sender, eventargs) => config.logger.Invoke(eventargs.Data);
            process.BeginOutputReadLine();
        }

        [Obsolete("Use Endpoint Instead")]
        public string Node
        {
            get { return Endpoint.ToString(); }
        }

        public EndPoint Endpoint
        {
            get {return new IPEndPoint(IPAddress.Loopback, config.port);}
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;


            try
            {
                process.CancelOutputRead();
                process.Kill();
                process.WaitForExit(2000);

                if (disposing)
                {
                    process.Dispose();
                    executable.Dispose();
                }
                
            }
            catch (Exception ex)
            {
                config.logger.Invoke(ex.ToString());
            }
            
            _disposed = true;
            
        }

        ~Redis()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool IsLinux()
        {
            var p = (int) Environment.OSVersion.Platform;

            return (p == 4) || (p == 6) || (p == 128);
        }

        private class Config : IConfig
        {
            private static readonly ConcurrentDictionary<int, byte> usedPorts = new ConcurrentDictionary<int, byte>();
            private static readonly Random random = new Random();
            internal Action<string> logger;
            internal int port;
            internal bool useExternalBinary;

            public Config()
            {
                do
                {
                    port = random.Next(49152, 65535 + 1);
                } while (usedPorts.ContainsKey(port));

                usedPorts.AddOrUpdate(port, i => byte.MinValue, (i, b) => byte.MinValue);
                logger = message => Trace.WriteLine(message);
            }

            public IConfig Port(int portNumber)
            {
                port = portNumber;
                return this;
            }

            public IConfig LogTo(Action<string> logFunction)
            {
                logger = logFunction;
                return this;
            }

            public IConfig UseExternalBinary()
            {
                useExternalBinary = true;
                return this;
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private readonly Process _process;
        private readonly Config _config = new Config();

        public Redis(Action<IConfig> configuration = null)
        {
            configuration?.Invoke(_config);

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            
            var assembly = Assembly.GetExecutingAssembly();
            var redisServerPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "Executables", isWindows ? "redis-server.exe" : "redis-server");

            var windowsArgs = isWindows ? "--persistence-available no" : "";
            var processStartInfo = new ProcessStartInfo(redisServerPath)
            {
                UseShellExecute = false,
                Arguments = $"--port {_config.port} --bind 127.0.0.1 {windowsArgs}",
                WindowStyle = ProcessWindowStyle.Maximized,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII
            };

            _process = Process.Start(processStartInfo);
            _process.ErrorDataReceived += (sender, eventargs) => _config.logger.Invoke(eventargs.Data);
            _process.OutputDataReceived += (sender, eventargs) => _config.logger.Invoke(eventargs.Data);
            _process.BeginOutputReadLine();
        }

        public EndPoint Endpoint
        {
            get {return new IPEndPoint(IPAddress.Loopback, _config.port);}
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;


            try
            {
                _process.CancelOutputRead();
                _process.Kill();
                _process.WaitForExit(2000);

                if (disposing)
                {
                    _process.Dispose();
                }
                
            }
            catch (Exception ex)
            {
                _config.logger.Invoke(ex.ToString());
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

        private class Config : IConfig
        {
            private static readonly ConcurrentDictionary<int, byte> usedPorts = new ConcurrentDictionary<int, byte>();
            private static readonly Random random = new Random();
            internal Action<string> logger;
            internal int port;

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
        }
    }
}

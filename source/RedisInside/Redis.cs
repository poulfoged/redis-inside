using System;
using System.Diagnostics;
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
            if (configuration != null)
                configuration(config);

            executable = new TemporaryFile(typeof(RessourceTarget).Assembly.GetManifestResourceStream(typeof(RessourceTarget), "redis-server.exe"), "exe");

            var processStartInfo = new ProcessStartInfo(" \"" + executable.Info.FullName + " \"")
            {
                UseShellExecute = false,
                Arguments = string.Format("--port {0} --bind 127.0.0.1 --persistence-available no", config.port),
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

        private class Config : IConfig
        {
            private static readonly Random random = new Random();
            internal Action<string> logger;
            internal int port;

            public Config()
            {
                port = random.Next(49152, 65535 + 1);
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

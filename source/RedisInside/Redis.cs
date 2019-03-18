using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using RedisInside.Executables;

namespace RedisInside
{
    /// <summary>
    /// Run integration-tests against Redis
    /// </summary>
    public class Redis : IDisposable
    {
        private bool _disposed;
        private readonly Process process = new Process();
        private readonly TemporaryFile executable;
        private readonly Config config = new Config();
        /// <summary>
        /// An event that will be signaled when the Redis server is ready to accept TCP connections.
        /// </summary>
        private readonly ManualResetEventSlim serverReady = new ManualResetEventSlim();

        public Redis(Action<IConfig> configuration = null)
        {
            if (configuration != null)
                configuration(config);

            executable = new TemporaryFile(typeof(RessourceTarget).Assembly.GetManifestResourceStream(typeof(RessourceTarget), "redis-server.exe"), "exe");

            process.StartInfo = new ProcessStartInfo(" \"" + executable.Info.FullName + " \"")
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

            process.ErrorDataReceived += (sender, eventargs) => config.logger.Invoke(eventargs.Data);
            process.OutputDataReceived += (sender, eventargs) => config.logger.Invoke(eventargs.Data);

            process.OutputDataReceived += DetectServerReady;

            void ProcessExitedHandler(object sender, EventArgs eventargs) =>
                throw new InvalidOperationException("The Redis process terminated unexpectedly.");

            process.EnableRaisingEvents = true; // The Exited event is only raised if this flag is set to `true` :)
            process.Exited += ProcessExitedHandler;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            WaitForRedisOrThrow();

            process.Exited -= ProcessExitedHandler;
            process.OutputDataReceived -= DetectServerReady;
        }

        /// <summary>
        /// Listens to stdout from the spawned instance of Redis and detects when it is ready to accept connections.
        /// </summary>
        private void DetectServerReady(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e?.Data) && e.Data.Contains("now ready to accept connections"))
            {
                serverReady.Set(); // Signal that it is now safe to consume this instance of Redis
            }
        }

        /// <summary>
        /// Waits up to 5 seconds for the Redis server to become available. Throws if Redis never reports that it is
        /// ready to start accepting connections within this time. Returns immediately once the server is ready.
        /// </summary>
        private void WaitForRedisOrThrow()
        {
            TimeSpan timeToWait = TimeSpan.FromSeconds(5);

            bool isServerReady = serverReady.Wait(timeToWait);

            if (!isServerReady)
            {
                var exnMessage = String.Format("The Redis server failed to become available after {0:0} seconds.", timeToWait.TotalSeconds);
                throw new InvalidOperationException(exnMessage);
            }
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

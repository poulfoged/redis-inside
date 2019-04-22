namespace RedisInside
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Run integration-tests against Redis
    /// </summary>
    public class Redis : IDisposable
    {
        private readonly Process process;
        private readonly TemporaryFile executable;
        private readonly IConfig config;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Redis"/> class.
        /// </summary>
        /// <param name="configuration">Configuration to override</param>
        public Redis(IConfig configuration = null)
        {
            this.config = configuration ?? new Config();

            ProcessStartInfo processStartInfo;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string executablePath = this.config.WindowsExecutablePath;
                if (string.IsNullOrEmpty(this.config.WindowsExecutablePath))
                {
                    this.executable = new TemporaryFile(this.GetType().Assembly.GetManifestResourceStream("RedisInside.Executables.redis-server.exe"), this.config, "exe");
                    executablePath = this.executable.Info.FullName;
                }

                processStartInfo = new ProcessStartInfo(" \"" + executablePath + " \"")
                {
                    UseShellExecute = false,
                    Arguments = string.Format("--port {0} --bind 127.0.0.1 --persistence-available no", this.config.Port),
                    WindowStyle = ProcessWindowStyle.Maximized,
                    CreateNoWindow = true,
                    LoadUserProfile = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.ASCII,
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string executablePath = this.config.LinuxExecutablePath;
                if (string.IsNullOrEmpty(this.config.LinuxExecutablePath))
                {
                    this.executable = new TemporaryFile(this.GetType().Assembly.GetManifestResourceStream("RedisInside.Executables.redis-server"), this.config);
                    executablePath = this.executable.Info.FullName;
                }

                processStartInfo = new ProcessStartInfo("/bin/bash")
                {
                    UseShellExecute = false,
                    Arguments = string.Format("-c \"{0} --port {1} --bind 127.0.0.1\"", executablePath, this.config.Port),
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                };
            }
            else
            {
                throw new Exception("Unsupported Platform");
            }

            this.process = Process.Start(processStartInfo);
            this.process.ErrorDataReceived += (sender, eventargs) => this.config.Log(eventargs.Data);
            this.process.OutputDataReceived += (sender, eventargs) => this.config.Log(eventargs.Data);
            this.process.BeginOutputReadLine();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Redis"/> class.
        /// </summary>
        ~Redis()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets endpoint of Redis Server
        /// </summary>
        public EndPoint Endpoint
        {
            get { return new IPEndPoint(IPAddress.Loopback, this.config.Port); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Is disposing?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                this.process.CancelOutputRead();
                this.process.Kill();
                this.process.WaitForExit(2000);

                if (disposing)
                {
                    this.process.Dispose();
                    this.executable?.Dispose();
                }
            }
            catch (Exception ex)
            {
                this.config.Log(ex.ToString());
            }

            this.disposed = true;
        }
    }
}

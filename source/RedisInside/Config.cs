namespace RedisInside
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    /// <summary>
    /// Default Configuration Class
    /// </summary>
    internal class Config : IConfig
    {
        private static readonly ConcurrentDictionary<int, byte> UsedPorts = new ConcurrentDictionary<int, byte>();
        private static readonly Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config()
        {
            do
            {
                this.Port = Random.Next(49152, 65535 + 1);
            }
            while (UsedPorts.ContainsKey(this.Port));

            UsedPorts.AddOrUpdate(this.Port, i => byte.MinValue, (i, b) => byte.MinValue);

            this.LinuxTemporaryPath = null;
            this.WindowsTemporaryPath = null;
            this.LinuxExecutablePath = null;
            this.WindowsExecutablePath = null;
        }

        /// <summary>
        /// Gets port of redis server
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets temporary path on Linux machine to copy executable
        /// </summary>
        public string LinuxTemporaryPath { get; }

        /// <summary>
        /// Gets temporary path on windows machine to copy executable
        /// </summary>
        public string WindowsTemporaryPath { get; }

        /// <summary>
        /// Gets path on Linux machine where executable is alreay present
        /// </summary>
        public string LinuxExecutablePath { get; }

        /// <summary>
        /// Gets path on Linux machine where executable is alreay present
        /// </summary>
        public string WindowsExecutablePath { get; }

        /// <summary>
        /// Logging method
        /// </summary>
        /// <param name="message">Message to Log</param>
        public void Log(string message)
        {
            Trace.WriteLine(message);
        }
    }
}

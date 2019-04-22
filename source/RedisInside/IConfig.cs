namespace RedisInside
{
    /// <summary>
    /// Interface for Configuration
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Gets port of redis server
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets temporary path on Linux machine to copy executable
        /// </summary>
        string LinuxTemporaryPath { get; }

        /// <summary>
        /// Gets temporary path on windows machine to copy executable
        /// </summary>
        string WindowsTemporaryPath { get; }

        /// <summary>
        /// Gets path on Linux machine where executable is alreay present
        /// </summary>
        string LinuxExecutablePath { get; }

        /// <summary>
        /// Gets path on Linux machine where executable is alreay present
        /// </summary>
        string WindowsExecutablePath { get; }

        /// <summary>
        /// Logging method
        /// </summary>
        /// <param name="message">Message to Log</param>
        void Log(string message);
    }
}
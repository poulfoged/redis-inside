namespace RedisInside
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Temporaty executable creation class
    /// </summary>
    public class TemporaryFile : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryFile"/> class.
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="extension">File extension</param>
        public TemporaryFile(IConfig config, string extension = "tmp")
        {
            string path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = config.WindowsTemporaryPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = Path.GetTempPath();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = config.LinuxTemporaryPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = "~/tmp";
                }

                Directory.CreateDirectory(path);
            }
            else
            {
                throw new Exception("Unsupported Platform");
            }

            this.Info = new FileInfo(Path.Combine(path, Guid.NewGuid().ToString("N") + "." + extension));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryFile"/> class.
        /// </summary>
        /// <param name="stream">Executable file stream</param>
        /// <param name="config">Configuration</param>
        /// <param name="extension">File extension</param>
        public TemporaryFile(Stream stream, IConfig config, string extension = "tmp")
            : this(config, extension)
        {
            using (stream)
            using (var destination = this.Info.OpenWrite())
            {
                stream.CopyTo(destination);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TemporaryFile"/> class.
        /// </summary>
        ~TemporaryFile()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets file info
        /// </summary>
        public FileInfo Info { get; }

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
                if (disposing)
                {
                    this.Info.Delete();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            this.disposed = true;
        }

        private void CopyTo(Stream result)
        {
            using (var stream = this.Info.OpenRead())
            {
                stream.CopyTo(result);
            }

            if (result.CanSeek)
            {
                result.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
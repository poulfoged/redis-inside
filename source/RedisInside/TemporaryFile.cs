using System;
using System.Diagnostics;
using System.IO;

namespace RedisInside
{
    public class TemporaryFile : IDisposable
    {
        private readonly FileInfo _fileInfo;
        private bool _disposed;

        public TemporaryFile(string extension = "tmp")
        {
            _fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "." + extension));
        }

        public TemporaryFile(Stream stream, string extension = "tmp") : this(extension)
        {
            using (stream)
            using (var destination = _fileInfo.OpenWrite())
                stream.CopyTo(destination);
        }

        public FileInfo Info 
        {
            get { return _fileInfo; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            try
            {
                if (disposing)
                    _fileInfo.Delete();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            _disposed = true;

        }

        ~TemporaryFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void CopyTo(Stream result)
        {
            using (var stream = _fileInfo.OpenRead())
                stream.CopyTo(result);

            if (result.CanSeek)
                result.Seek(0, SeekOrigin.Begin);
        }
    }
}
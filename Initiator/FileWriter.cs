using System;
using System.IO;
using System.Text;

namespace Initiator
{
    public class FileWriter : IDisposable
    {
        private Boolean _disposed;

        private static FileStream _stream;
        private static StreamWriter _streamWriter;


        public FileWriter(String filePath)
        {
            _stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
        }
        ~FileWriter()
        {
            Dispose(false);
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void WriteLine(String text)
        {
            _streamWriter.WriteLine(text);
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(Boolean disposing)
        {
            if (!_disposed)
            {
                ReleaseUnmanagedResources();
                if (disposing)
                    ReleaseManagedResources();

                _disposed = true;
            }
        }
        private void ReleaseUnmanagedResources()
        {
            // We didn't have it yet.
        }
        private void ReleaseManagedResources()
        {
            _streamWriter?.Dispose();
            _stream?.Dispose();
        }
    }
}
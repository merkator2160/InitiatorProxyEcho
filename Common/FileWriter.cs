using System;
using System.IO;
using System.Text;

namespace Common
{
    public class FileWriter : IDisposable
    {
        private static FileStream _stream;
        private static StreamWriter _streamWriter;


        public FileWriter(String filePath)
        {
            _stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void WriteLine(String text)
        {
            _streamWriter.WriteLine(text);
        }


        // IDisposable ////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            _streamWriter?.Dispose();
            _stream?.Dispose();
        }
    }
}
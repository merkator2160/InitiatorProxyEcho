using System;
using System.IO;
using System.Text;

namespace Common
{
    public class FileWriter
    {
        private readonly String _filePath;

        public FileWriter(String filePath)
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void WriteLine(String text)
        {
            using (var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(text);
                }
            }
        }
    }
}
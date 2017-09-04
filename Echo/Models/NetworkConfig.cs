using System;

namespace Echo.Models
{
    internal class NetworkConfig
    {
        public String ProxyHost { get; set; }
        public Int32 ProxyPort { get; set; }
        public Int32 NumberOfThreads { get; set; }
        public Int32 ReconnectDelay { get; set; }
    }
}
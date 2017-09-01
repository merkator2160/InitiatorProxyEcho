using System;
using Common.Enums;

namespace Common.Models
{
    [Serializable]
    public class Command
    {
        public CommandType Type { get; set; }
        public Byte[] Data { get; set; }
    }
}
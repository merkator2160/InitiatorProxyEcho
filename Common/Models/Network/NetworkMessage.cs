using Common.Models.Enums;
using System;

namespace Common.Models.Network
{
    [Serializable]
    public class NetworkMessage
    {
        public SerializableGuid ClientId { get; set; }
        public MessageType Type { get; set; }
        public Byte[] Data { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI.Protobuf
{

    public struct PacketHeader
    {
        private static int _headerSize = (sizeof(GamePacket.Proto.PacketType) + sizeof(int));
        public static int headerSize { get => _headerSize; }

        public GamePacket.Proto.PacketType packetType{ get; set; }        
        public int packetSize { get; set; }
    }
}

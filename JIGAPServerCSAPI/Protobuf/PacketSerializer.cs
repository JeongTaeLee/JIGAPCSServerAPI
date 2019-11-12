using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

namespace JIGAPServerCSAPI.Protobuf
{
    public class PacketSerializer
    {
        public static byte[] Serialize<T>(GamePacket.Proto.PacketType inType, T inPacket) where T : Google.Protobuf.IMessage
        {
            try
            {
                int packetSize = inPacket.CalculateSize();

                byte[] totalBuffer = new byte[PacketHeader.headerSize + packetSize];

                Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, totalBuffer, 0, sizeof(int));
                Buffer.BlockCopy(BitConverter.GetBytes((int)inType), 0, totalBuffer, sizeof(int), sizeof(int));
                Buffer.BlockCopy(inPacket.ToByteArray(), 0, totalBuffer, PacketHeader.headerSize, packetSize);

                return totalBuffer;
            }
            catch
            {
                throw;
            }
        }

        public static PacketHeader DeseriailizeHeader(byte[] inByte, int inOffset, int inCount)
        {
            int tempPacketSize = BitConverter.ToInt32(inByte, inOffset);
            GamePacket.Proto.PacketType tempPacketType = (GamePacket.Proto.PacketType)BitConverter.ToInt32(inByte, inOffset + sizeof(int));

            return new PacketHeader() { packetSize = tempPacketSize, packetType = tempPacketType };
        }

        public static T Deseriailize<T>(byte[] inByte, int inOffset, int inCount) where T : class, Google.Protobuf.IMessage, new()
        {
            try
            {   
                T returnPacket = new T();
                returnPacket = returnPacket.Descriptor.Parser.ParseFrom(inByte, inOffset, inCount) as T;
                return returnPacket;
            }

            catch
            {
                throw;
            }
        }
    }
}

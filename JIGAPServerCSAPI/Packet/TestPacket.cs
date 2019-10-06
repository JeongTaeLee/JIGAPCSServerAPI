using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public class TestPacket : BasePacket
    {
        public TestPacket()
        {
            _writingPosition += sizeof(int);
        }

        /// <summary>
        /// 패킷을 버퍼에 작성합니다. 만약 기존 패킷이있다면 뒤에 이어붙힙니다.
        /// </summary>
        /// <param name="inArray">작성할 패킷이 들어있는 배열입니다.</param>
        /// <param name="inOffset">배열에서 작성이 시작되는 위치입니다.</param>
        /// <param name="inCount">배열에서 작성할 크기 입니다. (index)</param>
        public void WritePacket(byte[] inArray, int inOffset, int inCount)
        {
            if (inArray == null)
                throw new ArgumentException("Param inArray is NULL");

            if (inOffset < 0 || inCount < 0)
                throw new ArgumentException("Param inOffset and inCount are invalid");

            Buffer.BlockCopy(inArray, inOffset, _bufferSegment.Array, _writingPosition, inCount);
            _writingPosition += inCount;

            WritePacketSize();
        }

        /// <summary>
        /// Packet Memory Buffer 첫 4Byte(int) 부분에 현재 패킷이 작성된 크기를 작성하는 함수입니다.
        /// </summary>
        private void WritePacketSize()
        {
            byte[] sizeBuffer = BitConverter.GetBytes(_writingPosition);
            Buffer.BlockCopy(sizeBuffer, 0, _bufferSegment.Array, 0, sizeBuffer.Length);
        }
    }
}

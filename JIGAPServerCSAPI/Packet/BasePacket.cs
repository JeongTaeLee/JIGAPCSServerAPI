using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public abstract class BasePacket
    {
        /// <summary>
        /// PacketMemoryPool에서 참조한 배열의 정보가 들어있는 구조체입니다.
        /// </summary>
        protected ArraySegment<byte> _bufferSegment;
        public ArraySegment<byte> buffer
        {
            get => _bufferSegment;
        }

        /// <summary>
        /// 현재 어디까지 쓰여졌는지를 저장하는 변수 입니다. Packet을 이어붙힐때 사용합니다.
        /// </summary>
        protected int _writingPosition = 0;
        public int writingPosition { get => _writingPosition; }


        /// <summary>
        /// Packet의 버퍼를 할당.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBuffer(byte[] inBuffer, int inOffset, int inCount)
        {
            if (inCount < 0 || inOffset < 0)
                throw new ArgumentException("Param inCount and inOffset are invalid");
            if ((inOffset + inCount) >= inBuffer.Length)
                throw new ArgumentOutOfRangeException();

            _bufferSegment = new ArraySegment<byte>(inBuffer, inOffset, inCount);
        }

        /// <summary>
        /// Packet 인스턴스 화 함.
        /// </summary>
        /// <typeparam name="T">인스턴스화 할 타입(BasePacket의 자식 클래스)</typeparam>
        /// <returns></returns>
        public static T Create<T>() where T : BasePacket, new()
        {
            T packet = new T();

            if (PacketMemoryPool.instance.SetBuffer(packet) == false)
                return null;

            return packet;
        }

        /// <summary>
        /// 인스턴스화된 패킷 삭제.
        /// </summary>
        /// <param name="inBasePacket"></param>
        public static void Destory(BasePacket inBasePacket)
        {
            if (inBasePacket == null)
                return;

            PacketMemoryPool.instance.FreeBuffer(inBasePacket);
        }
    }
}

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
        protected ArraySegment<byte> _buffer;
        public ArraySegment<byte> buffer
        {
            get => _buffer;
        }

        /// <summary>
        /// 현재 어디까지 쓰여졌는지를 저장하는 변수 입니다. Packet을 이어붙힐때 사용합니다.
        /// </summary>
        protected int _writePosition = 0;
        public int writePosition { get => _writePosition; }


        /// <summary>
        /// 사용할 배열을 지정합니다.
        /// </summary>
        /// <param name="inBuffer">사용할 배열 입니다.</param>
        /// <param name="inOffset">사용할 배열 부분의 시작 위치입니다.</param>
        /// <param name="inCount">사용할 배열의 크기 입니다.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBuffer(byte[] inBuffer, int inOffset, int inCount)
        {
            if (inCount < 0 || inOffset < 0)
                throw new ArgumentException("Param inCount and inOffset are invalid");
            if ((inOffset + inCount) >= inBuffer.Length)
                throw new ArgumentOutOfRangeException();

            _buffer = new ArraySegment<byte>(inBuffer, inOffset, inCount);
        }

        /// <summary>
        /// Packet을 클래스를 인스턴스화 합니다.
        /// </summary>
        /// <typeparam name="T">BasePacket을 상속받는 생성하고자 하는 클래스의 타입입니다</typeparam>
        /// <returns></returns>
        public static T Create<T>() where T : BasePacket, new()
        {
            T packet = new T();

            PacketMemoryPool.instance.SetBuffer(packet);

            return packet;
        }

        /// <summary>
        /// Create 되었던 Packet을 삭제합니다. Packet 버퍼는 다시 PacketMemoryPool에 되돌려집니다.
        /// </summary>
        /// <param name="inBasePacket"></param>
        public static void Destory(BasePacket inBasePacket)
        {
            if (inBasePacket == null)
                throw new ArgumentException("Param inBasePacket is NULL");

            PacketMemoryPool.instance.FreeBuffer(inBasePacket);
        }
    }
}

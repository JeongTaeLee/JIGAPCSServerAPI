using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    abstract class BasePacket 
    {
        protected byte[] _buffer = null;
        public byte[] buffer { get =>_buffer;  }

        protected int _offset = 0;
        public int offect { get => _offset; }

        protected int _count = 0;
        public int count { get => _count; }

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
                throw new ArgumentException();
            if ((inOffset + inCount) >= inBuffer.Length)
                throw new ArgumentOutOfRangeException();

            _offset = inOffset;
            _count = inCount;

            _buffer = inBuffer;
        }
    }
}

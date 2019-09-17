using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    class PacketMemoryPool
    {
        private byte[] _memoryBuffer = null;

        /// <summary>
        /// 한개의 EventArgs에 버퍼에 할당 가능한 크기입니다.
        /// </summary>
        private int _eachBufferSize = 0;

        /// <summary>
        /// 할당 가능한 버퍼의 시작 위치입니다.
        /// </summary>
        private int _currentIndex = 0;

        /// <summary>
        /// 총 버퍼 사이즈입니다.
        /// </summary>
        private int _totalBufferSize = 0;

        /// <summary>
        /// free된 메모리의 시작 위치가 보관되는 Stack입니다.
        /// </summary>
        private Stack<int> _freeIndexStack = new Stack<int>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inEachBufferSize">한개의 EventArgs에 버퍼에 할당 가능한 크기입니다.</param>
        /// <param name="inMaxBufferCount">총 버퍼의 사이즈입니다.</param>
        public PacketMemoryPool(int inEachBufferSize, int inTotalSize)
        {

            _eachBufferSize = inEachBufferSize;
            _totalBufferSize = inTotalSize;

            _memoryBuffer = new byte[inTotalSize];
        }

        public bool SetBuffer(BasePacket inPacket)
        {
            if (inPacket == null)
                throw new ArgumentNullException("[PacketMemoryPool.SetBuffer] 인자 inPacket이 NULL 입니다.");


            if (_freeIndexStack.Count > 0)
                inPacket.SetBuffer(_memoryBuffer, _freeIndexStack.Pop(), _eachBufferSize);
            else
            {
                if ((_totalBufferSize - _eachBufferSize) < _currentIndex)
                    return false;

                inPacket.SetBuffer(_memoryBuffer, _currentIndex, _eachBufferSize);
                _currentIndex += _eachBufferSize;
            }

            return true;
        }

        public void FreeBuffer(BasePacket inPacket)
        {
            if (inPacket == null)
                throw new ArgumentNullException("[PacketMemoryPool.FreeBuffer] 인자 inPacket이 NULL 입니다.");

            _freeIndexStack.Push(inPacket.offect);
            inPacket.SetBuffer(null, 0, 0);
        }

    }
}

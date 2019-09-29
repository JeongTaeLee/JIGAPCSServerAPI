using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public class PacketMemoryPool : MonoSingleton<PacketMemoryPool>
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
        ///  현재 PacketMemoryPool가 초기화 되었는지 확인하는 변수입니다.
        ///  만약 초기화 되지 않을 경우 사용시 예외를 던집니다.
        /// </summary>
        private bool _isInitialize = false;


        /// <summary>
        /// PacketMemoryPool을 초기화 합니다. 
        /// </summary>
        /// <param name="inEachBufferSize">BasePacket 하나의 할당할 사이즈입니다.</param>
        /// <param name="inTotalBufferSize">총 Buffer Pool의 사이즈입니다</param>
        public void InitializeMemoryPool(int inEachBufferSize, int inTotalBufferSize)
        {
            if (_isInitialize)
                return;

            if (0 > inEachBufferSize || 0 > inTotalBufferSize)
                throw new ArgumentException("Param inEachBufferSize and inTotalBufferSize are invalid");

            _eachBufferSize = inEachBufferSize;
            _totalBufferSize = inTotalBufferSize;

            _memoryBuffer = new byte[_totalBufferSize];

            _isInitialize = true;
        }

        public void ReleaseMemoryPool()
        {
            _memoryBuffer = null;
            _freeIndexStack.Clear();
            _isInitialize = false;
        }

        /// <summary>
        /// 인자로 전달된 패킷 변수의 Byte Buffer를 할당합니다.
        /// </summary>
        /// <param name="inPacket"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool SetBuffer(BasePacket inPacket)
        {
            if (_isInitialize == false)
                throw new Exception("You don't Intialize to PacketMemroy Pool Please PakcetMemoryPool.Instance.InitializeMemoryPool");

            if (inPacket == null)
                throw new ArgumentNullException("Param inArgs is NULL");

            // Free된 메모리 인덱스가 있다면.
            if (_freeIndexStack.Count > 0)
            {
                lock (_freeIndexStack)
                {
                    inPacket.SetBuffer(_memoryBuffer, _freeIndexStack.Pop(), _eachBufferSize);
                }
            }
            else
            {
                // 충분한 버퍼를 가지고 있지 않을 때.
                if ((_totalBufferSize - _eachBufferSize) < _currentIndex)
                    return false;

                inPacket.SetBuffer(_memoryBuffer, _currentIndex, _eachBufferSize);
                _currentIndex += _eachBufferSize;
            }


            return true;
        }

        /// <summary>
        /// 기존에 할당됐던 버퍼를 돌려줍니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public void FreeBuffer(BasePacket inPacket)
        {
            if (_isInitialize == false)
                throw new Exception("You don't Intialize to PacketMemroy Pool Please PakcetMemoryPool.Instance.InitializeMemoryPool");

            if (inPacket == null)
                throw new ArgumentException("Param inArgs is NULL.");

            if (inPacket.buffer.Array == null)
                return;

            lock (_freeIndexStack)
            {
                _freeIndexStack.Push(inPacket.buffer.Offset);
            }
            
            Array.Clear(_memoryBuffer, inPacket.buffer.Offset, inPacket.buffer.Count);
        }
    }
}

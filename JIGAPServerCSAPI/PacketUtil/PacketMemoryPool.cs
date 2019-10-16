using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    public class PacketMemoryPool : Singleton<PacketMemoryPool>
    {
        private byte[] _buffers = null;

        /// <summary>
        /// 한개의 EventArgs에 버퍼에 할당 가능한 크기입니다.
        /// </summary>
        private int _bufSetSize = 0;

        /// <summary>
        /// 할당 가능한 버퍼의 시작 위치입니다.
        /// </summary>
        private int _currentIndex = 0;

        /// <summary>
        /// 총 버퍼 사이즈입니다.
        /// </summary>
        private int _totalBufSize = 0;

        /// <summary>
        /// free된 메모리의 시작 위치가 보관되는 Stack입니다.
        /// </summary>
        private Stack<int> _freeIndexs = new Stack<int>();

        /// <summary>
        ///  현재 PacketMemoryPool가 초기화 되었는지 확인하는 변수입니다.
        ///  만약 초기화 되지 않을 경우 사용시 예외를 던집니다.
        /// </summary>
        private bool _completeInitialize = false;


        /// <summary>
        /// PacketMemoryPool를 초기화. 
        /// </summary>
        /// <param name="inBufSetSize">BasePacket 하나의 할당할 사이즈입니다.</param>
        /// <param name="inTotalBufSize">총 Buffer Pool의 사이즈입니다</param>
        public void InitializeMemoryPool(int inBufSetSize, int inTotalBufSize)
        {
            if (_completeInitialize)
                return;

            if (0 > inBufSetSize || 0 > inTotalBufSize)
                throw new ArgumentException("Param inEachBufferSize and inTotalBufferSize are invalid");

            _bufSetSize = inBufSetSize;
            _totalBufSize = inTotalBufSize;

            _buffers = new byte[_totalBufSize];

            _completeInitialize = true;
        }

        public void ReleaseMemoryPool()
        {
            _buffers = null;
            _freeIndexs.Clear();
            _completeInitialize = false;
        }

        /// <summary>
        /// 전달된 패킷에 버퍼 할당.
        /// </summary>
        /// <param name="inPacket"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool SetBuffer(Packet inPacket)
        {
            if (_completeInitialize == false)
                throw new Exception("You don't Intialize to PacketMemroy Pool Please PakcetMemoryPool.Instance.InitializeMemoryPool");

            if (inPacket == null)
                throw new ArgumentNullException("Param inArgs is NULL");

            // Free된 메모리 인덱스가 있다면.
            if (_freeIndexs.Count > 0)
            {
                lock (_freeIndexs)
                {
                    inPacket.SetBuffer(_buffers, _freeIndexs.Pop(), _bufSetSize);
                }
            }
            else
            {
                // 충분한 버퍼를 가지고 있지 않을 때.
                if ((_totalBufSize - _bufSetSize) < _currentIndex)
                    return false;

                inPacket.SetBuffer(_buffers, _currentIndex, _bufSetSize);
                _currentIndex += _bufSetSize;
            }


            return true;
        }

        /// <summary>
        /// 기존 패킷 할당됬던 버퍼 리턴.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public void FreeBuffer(Packet inPacket)
        {
            if (_completeInitialize == false)
                throw new Exception("You don't Intialize to PacketMemroy Pool Please PakcetMemoryPool.Instance.InitializeMemoryPool");

            if (inPacket == null)
                throw new ArgumentException("Param inArgs is NULL.");

            if (inPacket.buffer.Array == null)
                return;

            lock (_freeIndexs)
            {
                _freeIndexs.Push(inPacket.buffer.Offset);
            }
            
            Array.Clear(_buffers, inPacket.buffer.Offset, inPacket.buffer.Count);
        }
    }
}

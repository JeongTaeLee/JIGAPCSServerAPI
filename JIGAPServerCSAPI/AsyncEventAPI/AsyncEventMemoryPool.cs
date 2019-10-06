using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    public class AsyncEventMemoryPool
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


        /// <param name="inBufSetSize">한개의 EventArgs에 버퍼에 할당 가능한 크기입니다.</param>
        /// <param name="inTotalSize">총 버퍼의 사이즈입니다.</param>
        public AsyncEventMemoryPool(int inBufSetSize, int inTotalSize)
        {
            if (inBufSetSize < 0 || inTotalSize < 0)
                throw new ArgumentException("Param inEachBufferSize and inTotalSize are invalid");

            _bufSetSize = inBufSetSize;
            _totalBufSize = inTotalSize;

            _buffers = new byte[_totalBufSize];
        }

        public void ReleaseMemoryPool()
        {
            _freeIndexs.Clear();
            _buffers = null;
        }

        /// <summary>
        /// 인자로 전닯 받은 SocketAsyncEventArgs 객체에 메모리를 할당합니다.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public bool SetBuffer(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentNullException("Param inArgs is NULL");

            // Free된 메모리 인덱스가 있다면.
            if (_freeIndexs.Count > 0)
            {
                lock (_freeIndexs)
                {
                    inArgs.SetBuffer(_buffers, _freeIndexs.Pop(), _bufSetSize);
                }
            }
            else
            {
                // 충분한 버퍼를 가지고 있지 않을 때.
                if ((_totalBufSize - _bufSetSize) < _currentIndex)
                    return false;

                inArgs.SetBuffer(_buffers, _currentIndex, _bufSetSize);
                _currentIndex += _bufSetSize;
            }
            

            return true;
        }

        /// <summary>
        /// 인자로 들어온 SOcketAsyncEventArgs변수의 메모리를 AsyncEventMemoryPool에 돌려줍니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public void FreeBuffer(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentException("Param inArgs is NULL.");

            lock(_freeIndexs)
            {
                _freeIndexs.Push(inArgs.Offset);
            }

            inArgs.SetBuffer(null, 0, 0);
        }

    }
}

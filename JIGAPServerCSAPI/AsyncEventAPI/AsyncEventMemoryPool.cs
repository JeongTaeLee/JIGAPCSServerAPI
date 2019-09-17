using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    class AsyncEventMemoryPool
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
        public AsyncEventMemoryPool(int inEachBufferSize, int inTotalSize)
        {
            if (inEachBufferSize < 0 || inTotalSize < 0)
                throw new ArgumentException("[AsyncEventMemoryPool] 인자 inEachBufferSize 나 inTotalSize가 잘못되었습니다.");

            _eachBufferSize = inEachBufferSize;
            _totalBufferSize = inTotalSize;

            _memoryBuffer = new byte[_totalBufferSize];
        }

        /// <summary>
        /// 인자로 전닯 받은 SocketAsyncEventArgs 객체에 메모리를 할당합니다.
        /// </summary>
        /// <param name="inArgs"></param>
        /// <returns>true : 성공  false : 실패</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public bool SetBuffer(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentNullException("[AsyncEventMemoryPool.SetBuffer] 인자 inArgs가 NULL 입니다.");

            // Free된 메모리 인덱스가 있다면.
            if (_freeIndexStack.Count > 0)
            {
                lock (_freeIndexStack)
                {
                    inArgs.SetBuffer(_memoryBuffer, _freeIndexStack.Pop(), _eachBufferSize);
                }
            }
            else
            {
                // 충분한 버퍼를 가지고 있지 않을 때.
                if ((_totalBufferSize - _eachBufferSize) < _currentIndex)
                    return false;

                inArgs.SetBuffer(_memoryBuffer, _currentIndex, _eachBufferSize);
                _currentIndex += _eachBufferSize;
            }
            

            return true;
        }

        /// <summary>
        /// 기존에 할당됐던 버퍼를 돌려줍니다.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void FreeBuffer(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentNullException("[AsyncEventMemoryPool.FreeBuffer] 인자 inArgs가 NULL 입니다.");

            lock(_freeIndexStack)
            {
                _freeIndexStack.Push(inArgs.Offset);
            }

            inArgs.SetBuffer(null, 0, 0);
        }

    }
}

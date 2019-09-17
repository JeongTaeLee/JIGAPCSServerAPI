using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    class AsyncEventObjectPool
    {
        private Stack<SocketAsyncEventArgs> _asyncEventStack = null;

        public AsyncEventObjectPool(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentException("[AsyncEventObjectPool] 인자 inCapacity 가 잘못되었습니다.");

            _asyncEventStack = new Stack<SocketAsyncEventArgs>(inCapacity);
        }

        /// <summary>
        /// 컨테이너에 Object를 추가합니다.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Push(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentNullException("인자 inArgs가 NULL 입니다.");

            lock (_asyncEventStack)
            {
                _asyncEventStack.Push(inArgs);
            }
        }

        /// <summary>
        /// SocketAsyncArgs 하나 빼옵니다.
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Pop()
        {
            lock(_asyncEventStack)
            {
                return _asyncEventStack.Pop();
            }
        }

    }
}

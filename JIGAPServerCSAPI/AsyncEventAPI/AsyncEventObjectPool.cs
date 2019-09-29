using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    public class AsyncEventObjectPool
    {
        private Queue<SocketAsyncEventArgs> _asyncEventStack = null;

        public AsyncEventObjectPool(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentException("Param inCapacity is invalid");

            _asyncEventStack = new Queue<SocketAsyncEventArgs>(inCapacity);
        }
        public void ReleaseObjectPool()
        {
            _asyncEventStack.Clear();
            _asyncEventStack = null;
        }

        /// <summary>
        /// 컨테이너에 Object를 추가합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Push(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentException("Param inArgs is NULL");

            lock (_asyncEventStack)
            {
                _asyncEventStack.Enqueue(inArgs);
            }
        }

        /// <summary>
        /// SocketAsyncArgs 하나 빼옵니다.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public SocketAsyncEventArgs Pop()
        {
            if (_asyncEventStack.Count == 0)
                return null;

            lock(_asyncEventStack)
            {
                return _asyncEventStack.Dequeue();
            }
        }

    }
}

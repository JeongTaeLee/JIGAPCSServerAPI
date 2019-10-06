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
        private Queue<SocketAsyncEventArgs> _asyncEvents = null;

        public AsyncEventObjectPool(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentException("Param inCapacity is invalid");

            _asyncEvents = new Queue<SocketAsyncEventArgs>(inCapacity);
        }
        public void ReleaseObjectPool()
        {
            _asyncEvents.Clear();
            _asyncEvents = null;
        }

        /// <summary>
        /// 컨테이너에 Object를 추가합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Push(SocketAsyncEventArgs inArgs)
        {
            if (inArgs == null)
                throw new ArgumentException("Param inArgs is NULL");

            lock (_asyncEvents)
            {
                _asyncEvents.Enqueue(inArgs);
            }
        }

        /// <summary>
        /// SocketAsyncArgs 하나 빼옵니다.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public SocketAsyncEventArgs Pop()
        {
            if (_asyncEvents.Count == 0)
                return null;

            lock(_asyncEvents)
            {
                return _asyncEvents.Dequeue();
            }
        }

    }
}

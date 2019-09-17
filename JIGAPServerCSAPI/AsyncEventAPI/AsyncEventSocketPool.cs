using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    class AsyncEventSocketPool
    {
        private Stack<AsyncEventSocket> _asyncEventSocketStack = null;

        public AsyncEventSocketPool(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentException("[AsyncEventSocketPool] 인자 inCapacity 가 잘못되었습니다.");

            _asyncEventSocketStack = new Stack<AsyncEventSocket>(inCapacity);
        }

        /// <summary>
        /// 컨테이너에 Object를 추가합니다.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Push(AsyncEventSocket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentNullException("인자 inArgs가 NULL 입니다.");

            lock (_asyncEventSocketStack)
            {
                _asyncEventSocketStack.Push(inSocket);
            }
        }

        /// <summary>
        /// SocketAsyncArgs 하나 빼옵니다.
        /// </summary>
        /// <returns></returns>
        public AsyncEventSocket Pop()
        {
            lock (_asyncEventSocketStack)
            {
                return _asyncEventSocketStack.Pop();
            }
        }
    }
}

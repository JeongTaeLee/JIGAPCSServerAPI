﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    public class AsyncEventSocketPool
    {
        private Stack<AsyncEventSocket> _asyncEventSocketStack = null;

        public AsyncEventSocketPool(int inCapacity)
        {
            if (inCapacity <= 0)
                throw new ArgumentException("Param inCapacity is invalid");

            _asyncEventSocketStack = new Stack<AsyncEventSocket>(inCapacity);
        }

        public void ReleaseObjectPool()
        {
            _asyncEventSocketStack.Clear();
            _asyncEventSocketStack = null;
        }

        /// <summary>
        /// AsyncEventSocket Stack에서 인자로 전달된 Socket을 추가합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Push(AsyncEventSocket inSocket)
        {
            if (inSocket == null)
                return;

            lock (_asyncEventSocketStack)
            {
                _asyncEventSocketStack.Push(inSocket);
            }
        }

        /// <summary>
        /// AsyncEventSocket Stack에서 Socket 객체를 빼옵니다. 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public AsyncEventSocket Pop()
        { 
            if (_asyncEventSocketStack.Count == 0)
                return null;

            lock (_asyncEventSocketStack)
            {
                return _asyncEventSocketStack.Pop();
            }
        }
    }
}

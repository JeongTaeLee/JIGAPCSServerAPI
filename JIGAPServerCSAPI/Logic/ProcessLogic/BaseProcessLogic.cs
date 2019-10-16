using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.Logic
{
    public abstract class BaseProcessLogic
    {
        protected Action<string> _logPrinter = null;

        public abstract bool InitializeProcessLogic();
        public abstract void ReleaseProccesLogic();
        public abstract void OnDisconnectClient(BaseSocket inSocket);
        public abstract void OnConnectClient(BaseSocket inSocket);
        public abstract void OnRecv(BaseSocket inSocket, SocketAsyncEventArgs inArgs);

        protected void PrintLog(string inLog)
        {
            if (string.IsNullOrEmpty(inLog))
                throw new ArgumentException("Param inLog is Empty string and NULL");

            if (_logPrinter != null)
                _logPrinter(inLog);
        }
        
        public void SetLogPrinter(Action<string> inLogPrinter)
        {
            if (inLogPrinter == null)
                throw new ArgumentException("Param inLogPrinter is NULL");

            _logPrinter = inLogPrinter;
        }
    }
}

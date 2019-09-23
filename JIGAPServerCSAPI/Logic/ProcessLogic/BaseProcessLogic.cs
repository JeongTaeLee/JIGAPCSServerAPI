using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI.Logic
{
    public abstract class BaseProcessLogic
    {
        protected BaseServerLogic.LogPrinter _logPrinter = null;

        public abstract bool InitializeProcessLogic();
        public abstract void ReleaseProccesLogic();
        public abstract void OnDisconnectClient(BaseSocket inSocket);
        public abstract void OnConnectClient(BaseSocket inSocket);
        public abstract void OnProcess(BaseSocket inSocket, byte[] inPacket, int inOffset, int inCount);

        protected void PrintLog(string inLog)
        {
            if (string.IsNullOrEmpty(inLog))
                throw new ArgumentException("Param inLog is Empty string and NULL");

            if (_logPrinter != null)
                _logPrinter(inLog);
        }
        
        public void SetLogPrinter(BaseServerLogic.LogPrinter inLogPrinter)
        {
            if (inLogPrinter == null)
                throw new ArgumentException("Param inLogPrinter is NULL");

            _logPrinter = inLogPrinter;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.Logic
{
    public abstract class BaseProcessLogic<T> where T : BaseSocket
    { 
        public abstract bool InitializeProcessLogic();
        public abstract void ReleaseProccesLogic();
        public abstract void OnDisconnectClient(T inSocket);
        public abstract void OnConnectClient(T inSocket);
        public abstract void OnRecv(T inSocket, byte[] _buffer, int inOffset, int inByteTransferred);
    }
}

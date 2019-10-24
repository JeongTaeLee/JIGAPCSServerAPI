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
        public abstract bool InitializeProcessLogic();
        public abstract void ReleaseProccesLogic();
        public abstract void OnDisconnectClient(BaseSocket inSocket);
        public abstract void OnConnectClient(BaseSocket inSocket);
        public abstract void OnRecv(BaseSocket inSocket, SocketAsyncEventArgs inArgs);
    }
}

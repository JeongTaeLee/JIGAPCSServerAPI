using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JIGAPServerCSAPI;

namespace TestServer
{
    public class TestProcessLogic : JIGAPServerCSAPI.Logic.BaseProcessLogic
    {
        public override bool InitializeProcessLogic()
        {
            return true;
        }

        public override void OnConnectClient(BaseSocket inSocket)
        {
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket connectSocket
                = (JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket)inSocket;

            PrintLog($"[TestProcessLogic.OnConnectClient] connect to server");
        }

        public override void OnDisconnectClient(BaseSocket inSocket)
        {
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket connectSocket
                = (JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket)inSocket;

            PrintLog($"[TestProcessLogic.OnDisconnectClient] disconnect to server");
        }

        public override void OnProcess(BaseSocket inSocket, byte[] inPacket, int inOffset, int inCount)
        {
            
        }

        public override void ReleaseProccesLogic()
        {
            
        }
    }
}

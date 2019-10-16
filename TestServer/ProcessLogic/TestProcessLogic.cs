using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
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

        public override void OnRecv(BaseSocket inSocket, SocketAsyncEventArgs inArgs)
        {
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket socket = inSocket as JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket;
            socket.packetResolve.PacketCheck(inArgs.Buffer, inArgs.Offset, inArgs.BytesTransferred, PacketProcess);

            //string str = Encoding.UTF8.GetString(inArgs.Buffer, inArgs.Offset, inArgs.Count);
            //
            //PrintLog(str);
            //
            //TestPacket packet = BasePacket.Create<TestPacket>();
            //
            //packet.SettingPacket(inArgs.Buffer, inArgs.Offset, inArgs.Count);
            //socket.PushPacket(packet);
        }

        public void PacketProcess(byte[] inBuffer, int inOffset, int inCount)
        {
            string str = Encoding.UTF8.GetString(inBuffer, inOffset, inCount);
            PrintLog(str);
        }

        public override void ReleaseProccesLogic()
        {
            
        }
    }
}

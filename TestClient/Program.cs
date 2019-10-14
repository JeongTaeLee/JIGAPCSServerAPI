using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            JIGAPServerCSAPI.PacketMemoryPool.instance.InitializeMemoryPool(2048, 500000);
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket socket = new JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket(2048);
            socket.Connect("127.0.0.1", 9199);

            byte[] buffer = new byte[2048];
            string sendPacket = "Hello World";

            System.Net.Sockets.SocketAsyncEventArgs evt = new System.Net.Sockets.SocketAsyncEventArgs();
            evt.SetBuffer(buffer, 0, 2048);

            System.Net.Sockets.SocketAsyncEventArgs recv= new System.Net.Sockets.SocketAsyncEventArgs();
            socket.SetAsyncEvent(recv, evt);
            evt.UserToken = socket;
            evt.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleteEventCallBack);
            

            socket.SetSendCompleteSendProcess(OnSendCompleteEventCallBack);
  

            while (true)
            {

                TestPacket packet = TestPacket.Create<TestPacket>();

                if (packet != null)
                {
                    packet.Initialize();
                    socket.PushPacket(packet);
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        public static void OnSendCompleteEventCallBack(object inSender, SocketAsyncEventArgs inArgs)
        {
            try
            {
                if (inArgs.LastOperation == SocketAsyncOperation.Send)
                {
                    JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket socket = inArgs.UserToken as JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket;

                    // 전송을 완료했으므로 Packet을 뺍니다.
                    JIGAPServerCSAPI.BasePacket packet = socket.PopPacket();

                    // Austin Fix : 패킷 Pool 처리를 해서 패킷을 돌려주세요.
                    JIGAPServerCSAPI.BasePacket.Destory(packet);

                    // 다음 패킷이 있으면 다음 패킷을 보냅니다.
                    socket.SendNextPacket();

                   
                }
            }
            catch (SocketException ex)
            {

            }


        }
    }
}

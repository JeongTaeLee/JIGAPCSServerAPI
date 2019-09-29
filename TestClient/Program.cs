using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            JIGAPServerCSAPI.PacketMemoryPool.instance.InitializeMemoryPool(2048, 10000);
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket socket = new JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket();
            socket.Connect("127.0.0.1", 9199);

            byte[] buffer = new byte[2048];
            string sendPacket = "Hello World";

            System.Net.Sockets.SocketAsyncEventArgs evt = new System.Net.Sockets.SocketAsyncEventArgs();
            evt.SetBuffer(buffer, 0, 2048);

            System.Net.Sockets.SocketAsyncEventArgs recv= new System.Net.Sockets.SocketAsyncEventArgs();
            socket.SetAsyncEvent(recv, evt);

            /*
            JIGAPServerCSAPI.Packet.TestPacket packet = JIGAPServerCSAPI.BasePacket.Create<JIGAPServerCSAPI.Packet.TestPacket>();
            packet.WritePacket(Encoding.UTF8.GetBytes(sendPacket.ToArray<char>()), 0, sendPacket.Length);
            Array.Copy(packet.buffer.Array, packet.writePosition, packet.buffer.Array, packet.writePosition + 1, packet.writePosition);
            */
            TestPacket packet = TestPacket.Create<TestPacket>();
            packet.Initialize();

            socket.PushPacket(packet);    
            while (true)
            {
            }
        }
    }
}

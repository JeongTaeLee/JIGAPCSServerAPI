using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{ 
    class AsyncEventSocket : BaseSocket
    {   
        /// <summary>
        /// C# 소켓 클래스입니다.
        /// </summary>
        public Socket socket { get => _socket; }
        private Socket _socket = null;

        /// <summary>
        /// Recv용 Socket Event 변수입니다.
        /// </summary>
        public SocketAsyncEventArgs recvArgs { get => _recvArgs; }
        private SocketAsyncEventArgs _recvArgs = null;

        /// <summary>
        /// Recv용 Socket Event 변수입니다.
        /// </summary>
        public SocketAsyncEventArgs sendArgs { get => _sendArgs; }
        private SocketAsyncEventArgs _sendArgs = null;

        /// <summary>
        /// 전송할 패킷들을 한번에 한 개씩 전송하기 위한 컨테이너입니다.
        /// </summary>
        public Queue<BasePacket> sendPacketQueue { get => _sendPacketQueue; }
        private Queue<BasePacket> _sendPacketQueue = new Queue<BasePacket>(); 
        
    
        public AsyncEventSocket() { }

        /// <summary>
        /// 인자로 전달 받은 정보로 서버를 오픈합니다.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public override void StartSocket(string inIpAddress, int inPort, int inBlockingCount)
        {
            if (string.IsNullOrEmpty(inIpAddress))
                throw new ArgumentNullException("[AsyncEventSocket.StartSocket] 인자 inIpAddress 함수가 NULL입니다.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress;
            if (inIpAddress == "0.0.0.0")
                ipAddress = IPAddress.Any;
            else
                ipAddress = IPAddress.Parse(inIpAddress);

            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, inPort);

            _socket.Bind(ipEndPoint);
            _socket.Listen(inBlockingCount);
        }

        public override void CloseSocket()
        {
            _recvArgs = null;
            _sendArgs = null;

            _socket.Close();

            _socket = null;
        }

        /// <summary>
        /// 클라이언트측으로부터 연결 요청을 받아들입니다.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public override void Accept(BaseSocket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentNullException("[AsyncEventSocket.Accept] 인자 inSocket이 NULL입니다.");

            AsyncEventSocket asyncSocket = inSocket as AsyncEventSocket;

            if (asyncSocket != null)
                asyncSocket.SetSocket(_socket.Accept());
            else
                throw new ArgumentException("[AsyncEventSocket.Accept] 인자 형식이 잘못되었습니다.");
        }

        /// <summary>
        /// 비동기적으로 클라이언트로부터 연결 요청을 받아들입니다.
        /// </summary>
        /// <returns>true : 함수가 비동기 처리되고있습니다. false : 함수가 동기적으로 처리되었습니다(즉시 이벤트가 발생되었습니다)</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool AsyncAccept(SocketAsyncEventArgs args)
        {
            return _socket.AcceptAsync(args);
        }

        /// <summary>
        /// 인자로 받은 주소 정보로 서버에 연결 요청 합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException "></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override void Connect(string inIpAddress, int inPort)
        {
            IPAddress ipAddress;
            if (inIpAddress == "0.0.0.0")
                ipAddress = IPAddress.Any;
            else
                ipAddress = IPAddress.Parse(inIpAddress);

            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, inPort);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ipEndPoint);
        }

        /// <summary>
        /// AysncEvent 객체를 등록합니다.
        /// </summary>
        /// <param name="inRecvArgs"></param>
        /// <param name="inSendArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetAsyncEvent(SocketAsyncEventArgs inRecvArgs, SocketAsyncEventArgs inSendArgs)
        {
            if (inRecvArgs == null || inSendArgs == null)
                throw new ArgumentNullException("[AsyncEventSocket.SetAsyncEvent] 인자 inRecvArgs나 inSendArgs가 NULL입니다");

            _recvArgs = inRecvArgs;
            _sendArgs = inSendArgs;
        }
        public void SetSocket(Socket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentNullException("[AsyncEventSocket.SetSocket] 인지 inSocket이 NULL입니다.");

            _socket = null;
        }

        /// <summary>
        /// PacketQueue에 Packet을 추가합니다.
        /// </summary>
        /// <param name="inPacket"></param>
        /// <exception cref="ArgumentException"></exception>
        public void PushPacket(BasePacket inPacket)
        {
            if (inPacket == null)
                throw new ArgumentNullException("[AsyncEventSocket.PushPacket] 인자 inPacket NULL입니다.");

            lock (_sendPacketQueue)
            {
                _sendPacketQueue.Enqueue(inPacket);        
            }
        }

        public void PopPacket()
        {
            lock (_sendPacketQueue)
            {
                _sendPacketQueue.Dequeue();
            }
        }

        public BasePacket PeekPacket()
        {
            lock (_sendPacketQueue)
            { 
                return _sendPacketQueue.Peek();
            }
        }
    }
}

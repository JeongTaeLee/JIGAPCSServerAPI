using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    public class AsyncEventSocket : BaseSocket
    {
        public delegate void SendProcessFunc(object obj, SocketAsyncEventArgs args);

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
        private Queue<BasePacket> _sendPacketQueue = new Queue<BasePacket>(100);

        private SendProcessFunc _sendProcess = null;
    
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
                throw new ArgumentException("Param inIpAddress is NULL");

            if (inPort < 0)
                throw new ArgumentException("Param inPort is invalid");

            if (inBlockingCount <= 0)
                throw new ArgumentException("Param inBlockCount is invlild");

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

            if (_socket != null)
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
        public override void Accept(BaseSocket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentException("Param inSocket is NULL");

            AsyncEventSocket asyncSocket = inSocket as AsyncEventSocket;

            if (asyncSocket == null)
                throw new ArgumentException("Param Type is Invalid");

            asyncSocket.SetSocket(_socket.Accept());
        }

        /// <summary>
        /// 비동기적으로 클라이언트로부터 연결 요청을 받아들입니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        /// 비동기 송수신을 위한 SocketAsyncEventArgs를 셋팅합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void SetAsyncEvent(SocketAsyncEventArgs inRecvArgs, SocketAsyncEventArgs inSendArgs)
        {
            if (inRecvArgs == null || inSendArgs == null)
                throw new ArgumentException("Param inRecvArgs and inSendArgs are NULL");

            _recvArgs = inRecvArgs;
            _sendArgs = inSendArgs;
        }

        public void SetSocket(Socket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentException("Param inSocket is NULL");

            _socket = inSocket;
        }

        /// <summary>
        /// 비동기 전송을 수행하고 동기적으로 처리 되었을 때 호출할 전송 완료 함수를 셋팅합니다.
        /// 만약 이 함수가 지정되지 않을 경우. 동기적으로 초기화 됐을 때 Complete 함수가 호출이 되지 않습니다.. 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void SetSendCompleteSendProcess(SendProcessFunc inSendProcess)
        {
            if (inSendProcess == null)
                throw new ArgumentException("Param inSendProcess NULL");

            _sendProcess = inSendProcess;
        }

        /// <summary>
        /// 호출시 PacketQueue에 다음 패킷이 있다면 비동기 전송을 실행합니다
        /// </summary>
        public void SendNextPacket()
        {
            BasePacket packet = null;

            lock (_sendPacketQueue)
            {
                packet = _sendPacketQueue.Peek();
            }

            if (packet != null)
            {
                Array.Copy(packet.buffer.Array, 0, _sendArgs.Buffer, 0, packet.writePosition);

                if (_socket.SendAsync(_sendArgs) == false)
                {
                    if (_sendProcess != null)
                        _sendProcess(this, _sendArgs);
                }
            }
        }

        /// <summary>
        /// Packet Queue에 전송할 패킷을 추가합니다. 만약 Packet Queue가 비워있을 경우 바로 비동기 전송을 실행합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void PushPacket(BasePacket inPacket)
        {
            if (inPacket == null)
                throw new ArgumentException("Param inPacket is NULL");

                
            lock (_sendPacketQueue)
            {
                _sendPacketQueue.Enqueue(inPacket); 
            }

            SendNextPacket();
        }

        public BasePacket PopPacket()
        {
            BasePacket packet = null;

            lock (_sendPacketQueue)
            {
               packet = _sendPacketQueue.Dequeue();
            }

            return packet;
        }

        public BasePacket PeekPacket()
        {
            BasePacket packet = null;

            lock (_sendPacketQueue)
            {
                packet = _sendPacketQueue.Peek();
            }

            return packet;
        }
    }
}

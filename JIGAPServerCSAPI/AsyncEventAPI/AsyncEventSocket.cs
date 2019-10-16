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
        public Socket socket { get => _socket; }
        private Socket _socket = null;

        public SocketAsyncEventArgs recvArgs { get => _recvArgs; }
        private SocketAsyncEventArgs _recvArgs = null;

        public SocketAsyncEventArgs sendArgs { get => _sendArgs; }
        private SocketAsyncEventArgs _sendArgs = null;

        /// <summary>
        /// 순차 전달을 위한 전달 패킷 컨테이너
        /// </summary>
        public Queue<Packet> sendPackets { get => _sendPackets; }
        private Queue<Packet> _sendPackets = new Queue<Packet>(100);

        /// <summary>
        /// TCP 패킷 결합 담당 클래스
        /// </summary>
        private PacketResolve _packetResolve = null;
        public PacketResolve packetResolve { get => _packetResolve; }

        /// <summary>
        /// 동기적 패킷 전송 완료 시 호출 함수
        /// </summary>
        private Action<object, SocketAsyncEventArgs> _sendProcess = null;

        public AsyncEventSocket(int inMaxPacketSize) {
            _packetResolve = new PacketResolve(inMaxPacketSize); 

        }

        /// <summary>
        /// 인자로 받은 정보로 서버 시작
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
        /// 클라이언트 연결 요청 대기 함수.
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
        /// 비동기적 클라이언트 연결 요청 대기 함수.
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
        /// 인자로 받은 정보로 서버에 연결 요청 전송.
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

        public void SetSocket(Socket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentException("Param inSocket is NULL");

            _socket = inSocket;
        }

        public void SetAsyncEvent(SocketAsyncEventArgs inRecvArgs, SocketAsyncEventArgs inSendArgs)
        {
            if (inRecvArgs == null || inSendArgs == null)
                throw new ArgumentException("Param inRecvArgs and inSendArgs are NULL");

            _recvArgs = inRecvArgs;
            _sendArgs = inSendArgs;
        }

        public void SetSendCompleteSendProcess(Action<object, SocketAsyncEventArgs> inSendProcess)
        {
            if (inSendProcess == null)
                throw new ArgumentException("Param inSendProcess NULL");

            _sendProcess = inSendProcess;
        }

        public void SendNextPacket()
        {
            Packet packet = null;

            lock (_sendPackets)
            {
                if (_sendPackets.Count != 0)
                    packet = this.PeekPacket();
            }

            if (packet != null)
            {
                Array.Copy(packet.buffer.Array, packet.buffer.Offset, _sendArgs.Buffer, 0, packet.writingPosition);

                _sendArgs.SetBuffer(_sendArgs.Buffer, _sendArgs.Offset, packet.writingPosition);
                bool panding = _socket.SendAsync(_sendArgs);

                if (panding == false)
                {
                    if (_sendProcess != null)
                        _sendProcess(this, _sendArgs);
                }
            }
        }

        public void PushPacket(Packet inPacket)
        {
            if (inPacket == null)
                throw new ArgumentException("Param inPacket is NULL");
                
            lock (_sendPackets)
            {
                _sendPackets.Enqueue(inPacket); 
            }

            if (_sendPackets.Count == 1)
            {
                Array.Copy(inPacket.buffer.Array, inPacket.buffer.Offset, _sendArgs.Buffer, 0, inPacket.writingPosition);

                _sendArgs.SetBuffer(_sendArgs.Buffer, _sendArgs.Offset, inPacket.writingPosition);
                bool panding = _socket.SendAsync(_sendArgs);

                if (panding == false)
                {
                    if (_sendProcess != null)
                        _sendProcess(this, _sendArgs);
                }
            }
        }

        public Packet PopPacket()
        {
            if (_sendPackets.Count == 0)
                return null;

            Packet packet = null;

            lock (_sendPackets)
            {
               packet = _sendPackets.Dequeue();
            }

            return packet;
        }

        public Packet PeekPacket()
        {
            if (_sendPackets.Count == 0)
                return null;
                 

            Packet packet = null;

            lock (_sendPackets)
            {
                packet = _sendPackets.Peek();
            }

            return packet;
        }
    }
}

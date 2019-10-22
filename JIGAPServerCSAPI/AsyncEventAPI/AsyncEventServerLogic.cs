using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    public class AsyncEventServerLogic : Logic.BaseServerLogic
    {
        private AsyncEventSocket _serverSocket = null;
        public AsyncEventSocket serverSocket { get => _serverSocket; }

        private AsyncEventMemoryPool _asyncEventMemoryPool = null;
        private AsyncEventSocketPool _asyncEventSocketPool = null;
        private AsyncEventObjectPool _recvAsyncEventPool = null;
        private AsyncEventObjectPool _sendAsyncEventPool = null;

        public AsyncEventServerLogic(Logic.BaseProcessLogic inProcessLogic)
            :base(inProcessLogic)
        {}

        /// <summary>
        /// 호출 시 서버 초기화와 서버 실행을 하는 함수입니다.
        /// </summary>
        /// <param name="inIpAddress">실행할 서버의 IP 주소입니다.</param>
        /// <param name="inPort">실행할 서버의 PORT 주소입니다.</param>
        /// <param name="inListenBlocking">실행할 서버의 최대 대기열 수입니다.</param>
        /// <returns></returns>
        public override bool StartServer(string inIpAddress, int inPort, int inListenBlocking)
        {
            try
            {
                _serverSocket = new AsyncEventSocket(_packetMaxSize);
                _serverSocket.StartSocket(inIpAddress, inPort, inListenBlocking);
                
                PacketMemoryPool.instance.InitializeMemoryPool(_packetMaxSize, _packetMaxSize * _userMaxCount);
                
                _asyncEventMemoryPool = new AsyncEventMemoryPool(_packetMaxSize, (_packetMaxSize * _userMaxCount) * 2);
                
                _asyncEventSocketPool = new AsyncEventSocketPool(_userMaxCount);
                for (int i = 0; i < _userMaxCount; ++i)
                {
                    AsyncEventSocket _socket = new AsyncEventSocket(_packetMaxSize);
                    _socket.SetSendCompleteSendProcess(OnSendCompleteEventCallBack);
                
                    _asyncEventSocketPool.Push(_socket);
                }
                
                _recvAsyncEventPool = new AsyncEventObjectPool(_userMaxCount);
                for (int i = 0; i < _userMaxCount; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleteEventCallBack);
                
                
                    if (_asyncEventMemoryPool.SetBuffer(args) == false)
                        throw new Exception("asyncEventMemoryPool Size Over");
                
                    _recvAsyncEventPool.Push(args);
                }
                
                _sendAsyncEventPool = new AsyncEventObjectPool(_userMaxCount);
                for (int i = 0; i < _userMaxCount; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleteEventCallBack);
                
                    if (_asyncEventMemoryPool.SetBuffer(args) == false)
                        throw new Exception("asyncEventMemoryPool Size Over");
                
                    _sendAsyncEventPool.Push(args);
                }
                
                StartAcceptThread();

                _isServerOn = true;

                PrintLog("Server started successfully");
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                PrintLog($"Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
                PrintLog(ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 호출시 서버를 종료합니다.
        /// </summary>
        public override void EndServer()
        {
            _isServerOn = false;

            _serverSocket?.CloseSocket();

            JoinAcceptThread();
            
            _serverSocket = null;

            _asyncEventSocketPool?.ReleaseObjectPool();
            _asyncEventSocketPool = null;

            _recvAsyncEventPool?.ReleaseObjectPool();
            _recvAsyncEventPool = null;

            _sendAsyncEventPool?.ReleaseObjectPool();
            _sendAsyncEventPool = null;

            _asyncEventMemoryPool?.ReleaseMemoryPool();
            _asyncEventMemoryPool = null;

            PacketMemoryPool.instance.ReleaseMemoryPool();

            _processLogic?.ReleaseProccesLogic();
            _processLogic = null;

            PrintLog("Server ended successfully");
        }

        /// <summary>
        /// 클라이언트에 연결 요청을 담당하는 함수입니다. 쓰레드로 작동합니다.
        /// </summary>
        public override void AcceptTask()
        {
            while (true)
            {
                base.AcceptTask();

                AsyncEventSocket newClientSocket = null;
            
                try
                { 
                    newClientSocket = _asyncEventSocketPool.Pop();

                    if (newClientSocket == null)
                    {
                        PrintLog("[AsyncEventServerLogic.AcceptTast] Can't pop to _asyncEventSocketPool");
                        continue;
                    }

                    serverSocket.Accept(newClientSocket);

                    if (newClientSocket.socket.Connected)
                    {
                        SocketAsyncEventArgs recvArgs = _recvAsyncEventPool.Pop();
                        if (recvArgs == null)
                        {
                            PrintLog("[AsyncEventServerLogic.AcceptTast] Can't pop to _recvAsyncEventPool");

                            newClientSocket.CloseSocket();

                            _asyncEventSocketPool.Push(newClientSocket);
                            continue;
                        }

                        recvArgs.UserToken = newClientSocket;

                        SocketAsyncEventArgs sendArgs = _sendAsyncEventPool.Pop();
                        if (sendArgs == null)
                        {
                            PrintLog("[AsyncEventServerLogic.AcceptTast] Can't pop to _recvAsyncEventPool ");

                            newClientSocket.CloseSocket();

                            _asyncEventSocketPool.Push(newClientSocket);
                            _recvAsyncEventPool.Push(recvArgs);

                            continue;
                        }

                        sendArgs.UserToken = newClientSocket;

                        newClientSocket.SetAsyncEvent(recvArgs, sendArgs);

                        processLogic.OnConnectClient(newClientSocket);

                        if (newClientSocket.socket.ReceiveAsync(recvArgs) == false)
                            OnRecvCompleteEventCallBack(this, recvArgs);
                    }          
                    else
                        _asyncEventSocketPool.Push(newClientSocket);

                }
                catch (SocketException ex)
                {
                    if (_isServerOn == false)
                        return;

                    PrintLog($"[Socket Error : {ex.SocketErrorCode} / {ex.TargetSite}] : {ex.Message}");

                    OnCloseSocket(newClientSocket);
                }
                catch (Exception ex)
                {
                    if (_isServerOn == false)
                        return;

                    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                    PrintLog($"Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
                    PrintLog(ex.StackTrace);

                    OnCloseSocket(newClientSocket);
                }

            }
        }

        /// <summary>
        /// SocketAsyncEventArgs에서 Recv가 완료될 시 호출되는 CallBack 함수입니다.
        /// </summary>
        public void OnRecvCompleteEventCallBack(object inSender, SocketAsyncEventArgs inArgs)
        {
            try
            {
                if (inArgs == null)
                    return;

                if (inArgs.LastOperation == SocketAsyncOperation.Receive)
                {
                    if (inArgs.BytesTransferred > 0 && inArgs.SocketError == SocketError.Success)
                    {
                        AsyncEventSocket token = inArgs.UserToken as AsyncEventSocket;

                        _processLogic.OnRecv(token, inArgs);

                        bool isPending = token.socket.ReceiveAsync(inArgs);

                        if (isPending == false)
                            OnRecvCompleteEventCallBack(this, inArgs);
                    }
                    else
                    {
                        if (_isServerOn == false)
                            return;

                        if (inArgs == null)
                            return;

                        AsyncEventSocket closeSocket = inArgs.UserToken as AsyncEventSocket;

                        _processLogic.OnDisconnectClient(closeSocket);

                        OnCloseSocket(closeSocket);
                    }
                }

            }
            catch (Exception ex)
            {
                if (_isServerOn == false)
                    return;

                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                PrintLog($"Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
                PrintLog(ex.StackTrace);

                AsyncEventSocket errorSocket = inArgs.UserToken as AsyncEventSocket;
                _processLogic.OnDisconnectClient(errorSocket);
                OnCloseSocket(errorSocket);
            }



        }

        /// <summary>
        /// SocketAsyncEventArgs에서 Recv가 완료될 시 호출되는 CallBack 함수입니다.
        /// </summary>
        public void OnSendCompleteEventCallBack(object inSender, SocketAsyncEventArgs inArgs)
        {
            try
            { 
                if (inArgs.LastOperation == SocketAsyncOperation.Send)
                {
                    AsyncEventSocket socket = inArgs.UserToken as AsyncEventSocket;

                    // 전송을 완료했으므로 Packet을 뺍니다.
                    Packet packet = socket.PopPacket();

                    // Austin Fix : 패킷 Pool 처리를 해서 패킷을 돌려주세요.
                    Packet.Destory(packet);

                    // 패킷을 보낸 후 사이즈를 버퍼 사이즈를 정삭적으로 돌려놓습니다.
                    inArgs.SetBuffer(inArgs.Buffer, inArgs.Offset, _packetMaxSize);

                    // 다음 패킷이 있으면 다음 패킷을 보냅니다.
                    socket.SendNextPacket();
                }
            }
            catch (Exception ex)
            {
                if (_isServerOn == false)
                    return;

                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                PrintLog($"Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
                PrintLog(ex.StackTrace);

                _processLogic.OnDisconnectClient(inArgs.UserToken as AsyncEventSocket);
                OnCloseSocket(inArgs.UserToken as AsyncEventSocket);
            }

            
        }
        /// <summary>
        /// 인자로 전달 된 소켓의 종료 처리를 하는 함수입니다.
        /// </summary>
        public void OnCloseSocket(AsyncEventSocket inSocket)
        {
            if (inSocket == null)
                return;

            inSocket.packetResolve.Clear();

            _recvAsyncEventPool.Push(inSocket.recvArgs);
            _sendAsyncEventPool.Push(inSocket.sendArgs);

            inSocket.CloseSocket();

            _asyncEventSocketPool.Push(inSocket);
        }
    }
}

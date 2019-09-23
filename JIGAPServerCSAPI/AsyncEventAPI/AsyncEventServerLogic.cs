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

        protected override void ManagedDispose()
        {
            //Austin Fix : GC 콜렉터 작업 진행해야합니다.
            _serverSocket = null;

            _asyncEventMemoryPool = null;
            _asyncEventSocketPool = null;
            _recvAsyncEventPool = null;
            _sendAsyncEventPool = null;

            base.ManagedDispose();
        }

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
                _serverSocket = new AsyncEventSocket();
                _serverSocket.StartSocket(inIpAddress, inPort, inListenBlocking);

                PacketMemoryPool.instance.InitializeMemoryPool(AsyncEventDefine._pakcetSize, AsyncEventDefine._pakcetSize * AsyncEventDefine._maxUserCount);

                _asyncEventMemoryPool = new AsyncEventMemoryPool(AsyncEventDefine._pakcetSize, AsyncEventDefine._pakcetSize * AsyncEventDefine._maxUserCount);
                
                
                _asyncEventSocketPool = new AsyncEventSocketPool(AsyncEventDefine._maxUserCount);
                for (int i = 0; i < AsyncEventDefine._maxUserCount; ++i)
                {
                    AsyncEventSocket _socket = new AsyncEventSocket();

                    _socket.SetSendCompleteSendProcess(OnSendCompleteEventCallBack);

                    _asyncEventSocketPool.Push(new AsyncEventSocket());
                    
                }

                _recvAsyncEventPool = new AsyncEventObjectPool(AsyncEventDefine._maxUserCount);
                for (int i = 0; i < AsyncEventDefine._maxUserCount; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleteEventCallBack);

                    _asyncEventMemoryPool.SetBuffer(args);

                    _recvAsyncEventPool.Push(args);
                }

                _sendAsyncEventPool = new AsyncEventObjectPool(AsyncEventDefine._maxUserCount);
                for (int i = 0; i < AsyncEventDefine._maxUserCount; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleteEventCallBack);

                    _asyncEventMemoryPool.SetBuffer(args);

                    _sendAsyncEventPool.Push(args);
                }

                StartAcceptThread();

                _isServerOn = true;

                PrintLog("Server started successfully");
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                string fileName = stackTrace.GetFrame(0).GetFileName();
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                PrintLog($"[{fileName} Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// 호출시 서버를 종료합니다.
        /// </summary>
        public override void EndServer()
        {
            _isServerOn = false;

            _serverSocket.CloseSocket();

            JoinAcceptThread();

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

                    serverSocket.Accept(newClientSocket);

                    if (newClientSocket.socket.Connected)
                    {
                        SocketAsyncEventArgs recvArgs = _recvAsyncEventPool.Pop();
                        recvArgs.UserToken = newClientSocket;

                        SocketAsyncEventArgs sendArgs = _sendAsyncEventPool.Pop();
                        sendArgs.UserToken = newClientSocket;

                        newClientSocket.SetAsyncEvent(recvArgs, sendArgs);

                        processLogic.OnConnectClient(newClientSocket);

                        bool pending = newClientSocket.socket.ReceiveAsync(recvArgs);
                        if (pending == false)
                            OnRecvCompleteEventCallBack(this, recvArgs);
                    }                    
                }
                catch (Exception ex)
                {
                    if (_isServerOn == false)
                        return;

                    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                    string fileName = stackTrace.GetFrame(0).GetFileName();
                    fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    PrintLog($"[{fileName} Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
                    
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
                if (inArgs.LastOperation == SocketAsyncOperation.Receive)
                {
                    if (inArgs.BytesTransferred > 0 && inArgs.SocketError == SocketError.Success)
                    {
                        _processLogic.OnProcess(inArgs.UserToken as BaseSocket, inArgs.Buffer, inArgs.Offset, inArgs.BytesTransferred);

                        AsyncEventSocket token = inArgs.UserToken as AsyncEventSocket;

                        if (token == null)
                            throw new ArgumentException("Param inArgs is Type Error");

                        bool pending = token.socket.ReceiveAsync(inArgs);

                        if (pending == false)
                            OnRecvCompleteEventCallBack(this, inArgs);
                    }
                    else
                    {
                        if (_isServerOn == false)
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

                string fileName = stackTrace.GetFrame(0).GetFileName();
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                PrintLog($"[{fileName} Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
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

                    if (socket == null)
                        throw new ArgumentException("Param inArgs is Type Error");

                    // 전송을 완료했으므로 Packet을 뺍니다.
                    BasePacket packet = socket.PopPacket();

                    // Austin Fix : 패킷 Pool 처리를 해서 패킷을 돌려주세요.
                    BasePacket.Destory(packet);

                    // 다음 패킷이 있으면 다음 패킷을 보냅니다.
                    socket.SendNextPacket();
                }
            }
            catch (Exception ex)
            {
                if (_isServerOn == false)
                    return;

                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ex, true);

                string fileName = stackTrace.GetFrame(0).GetFileName();
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                PrintLog($"[{fileName} Line : {stackTrace.GetFrame(0).GetFileLineNumber()}] : { ex.Message}");
            }
        }
        /// <summary>
        /// 인자로 전달 된 소켓의 종료 처리를 하는 함수입니다.
        /// </summary>
        public void OnCloseSocket(AsyncEventSocket inSocket)
        {
            if (inSocket == null)
                throw new ArgumentNullException("Param inSocket is NULL");

            _recvAsyncEventPool.Push(inSocket.recvArgs);
            _sendAsyncEventPool.Push(inSocket.sendArgs);

            inSocket.CloseSocket();

            _asyncEventSocketPool.Push(inSocket);
        }
    }
}

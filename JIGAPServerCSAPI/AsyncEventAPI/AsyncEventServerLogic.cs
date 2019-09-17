using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI.AsyncEventAPI
{
    class AsyncEventServerLogic : Logic.BaseServerLogic<Logic.TestProcessLogic>
    {
        private AsyncEventSocket _serverSocket = null;
        public AsyncEventSocket serverSocket { get => _serverSocket; }

        private AsyncEventMemoryPool _asyncEventMemoryPool = null;
        private AsyncEventSocketPool _asyncEventSocketPool = null;
        private AsyncEventObjectPool _recvAsyncEventPool = null;
        private AsyncEventObjectPool _sendAsyncEventPool = null;

        public override bool StartServer(string inIpAddress, int inPort, int inListenBlocking)
        {
            try
            {
                _serverSocket = new AsyncEventSocket();
                _serverSocket.StartSocket(inIpAddress, inPort, inListenBlocking);

                _asyncEventMemoryPool = new AsyncEventMemoryPool(1024, 1024 * 40000);

                _asyncEventSocketPool = new AsyncEventSocketPool(4000);
                for (int i = 0; i < 4000; ++i)
                    _asyncEventSocketPool.Push(new AsyncEventSocket());

                _recvAsyncEventPool = new AsyncEventObjectPool(40000);
                for (int i = 0; i < 40000; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleteEventCallBack);

                    _asyncEventMemoryPool.SetBuffer(args);
                }

                _sendAsyncEventPool = new AsyncEventObjectPool(40000);
                for (int i = 0; i < 40000; ++i)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    _asyncEventMemoryPool.SetBuffer(args);
                }

                StartAcceptThread();
            }
            catch (ArgumentException ex)
            {

            }
            catch (SocketException ex)
            {
                
            }
            catch (FormatException ex)
            {

            }
            catch (ObjectDisposedException ex)
            {

            }
            catch (System.Security.SecurityException)
            {

            }

            return true;
        }

        public override void EndServer()
        {
            throw new NotImplementedException();
        }

        public override void AcceptTask()
        {
            while (true)
            {
                base.AcceptTask();

                try
                { 
                    AsyncEventSocket newClientSocket = _asyncEventSocketPool.Pop();

                    serverSocket.Accept(newClientSocket);

                    if (newClientSocket.socket.Connected)
                    {
                        SocketAsyncEventArgs recvArgs = _recvAsyncEventPool.Pop();
                        recvArgs.UserToken = newClientSocket;

                        SocketAsyncEventArgs sendArgs = _sendAsyncEventPool.Pop();
                        sendArgs.UserToken = newClientSocket;

                        newClientSocket.SetAsyncEvent(recvArgs, sendArgs);

                        bool pending = newClientSocket.socket.ReceiveAsync(recvArgs);
                        if (!pending)
                            RecvProcess(recvArgs);
                    }                    
                }
                catch(SocketException ex)
                {

                }
            }
        }

        public void OnRecvCompleteEventCallBack(object inSender, SocketAsyncEventArgs inArgs)
        {
            if (inArgs.LastOperation == SocketAsyncOperation.Receive)
            {
                RecvProcess(inArgs);
                return;
            }
        }

        public void RecvProcess(SocketAsyncEventArgs inArgs)
        { 
            if (inArgs.BytesTransferred > 0 && inArgs.SocketError == SocketError.Success)
            {
                AsyncEventSocket token = inArgs.UserToken as AsyncEventSocket;

                // 수신 작업이 끝나면 다시 보냅니다.
                bool pending = token.socket.ReceiveAsync(inArgs);
                if (!pending)
                    RecvProcess(inArgs);
            }
            else
            {
                AsyncEventSocket closeSocket = inArgs.UserToken as AsyncEventSocket;
                OnCloseSocket(closeSocket);
            }
        }

        public void OnCloseSocket(AsyncEventSocket inSocket)
        {
            _recvAsyncEventPool.Push(inSocket.recvArgs);
            _sendAsyncEventPool.Push(inSocket.sendArgs);

            inSocket.CloseSocket();

            _asyncEventSocketPool.Push(inSocket);
        }
    }
}

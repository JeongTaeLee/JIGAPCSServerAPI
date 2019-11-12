using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JIGAPServerCSAPI.Logic
{
    public abstract class BaseServerLogic<SocketType> where SocketType : BaseSocket
    {
        protected BaseProcessLogic<SocketType> _processLogic = null;
        protected BaseProcessLogic<SocketType> processLogic { get => _processLogic; }

        protected Task _acceptTask = null;
        protected Task _ioTask = null;

        protected bool _isServerOn = false;
        public bool isServerOn { get => _isServerOn; }

        protected int _userMaxCount = 100000;

        protected int _packetMaxSize = 2048;
        

        public BaseServerLogic(BaseProcessLogic<SocketType> inProcessLogic)
        {
            _processLogic = inProcessLogic;
            _processLogic.InitializeProcessLogic();
        }

        public abstract bool StartServer(string inIpAddress, int inPort, int inListenBlocking);
        public abstract void EndServer();

        public virtual void AcceptTask() {}
        public virtual void IOThread() {}

        /// <summary>
        /// Accept Thread를 시작합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ThreadStartException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        protected void StartAcceptThread()
        {
            _acceptTask = Task.Run(() => { AcceptTask(); });
        }

        /// <summary>
        /// IO Thread를 시작합니다.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ThreadStateException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        protected void StartIOThread()
        {
            _acceptTask = Task.Run(() => { IOThread(); });
        }

        /// <summary>
        /// Accept Thread가 종료할 때 까지 대기합니다.
        /// </summary>
        /// <exception cref="ThreadStateException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        protected void JoinAcceptThread()
        {
            _acceptTask?.Wait();
        }

        /// <summary>
        /// IO Thread가 종료할 때 까지 대기합니다.
        /// </summary>
        /// <exception cref="ThreadStateException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        protected void JoinIOThread()
        {
            _ioTask?.Wait();
        }

        /// <summary>
        /// 로그를 출력 할 수 있는 함수 포인터 변수를 셋팅합니다.
        /// </summary>
        /// <param name="inLogPrinter"></param>
        public void SetLogPrinter(Action<string[]> inLogPrinter)
        {
            if (inLogPrinter == null)
                throw new ArgumentException("Param inLogPrinter is NULL");

            Loger.Initialzie(inLogPrinter);
        }

       
        public virtual void  SetUserCount(int inUserCount)
        {
            _userMaxCount = inUserCount;
        }

        public virtual void SetPaketSize(int inPacketSize)
        {
            _packetMaxSize = inPacketSize;
        }
    }
}

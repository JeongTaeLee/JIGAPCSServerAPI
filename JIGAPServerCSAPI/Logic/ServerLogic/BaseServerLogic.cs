using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JIGAPServerCSAPI.Logic
{
    abstract class BaseServerLogic<ProcessLogic> where ProcessLogic : BaseProcessLogic, new() 
    {
        /// <summary>
        /// 패킷 처리 작업 등을 수행할 ProcessLogic입니다 BaseProcessLogic 상속받습니다.
        /// </summary>
        protected ProcessLogic _processLogic = null;
        public ProcessLogic processLogic { get => _processLogic; }

        protected Thread _acceptThread = null;
        protected Thread _ioThread = null;

        public BaseServerLogic()
        {
            _processLogic = new ProcessLogic();
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
            _acceptThread = new Thread(new ThreadStart(AcceptTask));
            _acceptThread.Start();
            Thread.Sleep(0);
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
            _ioThread = new Thread(new ThreadStart(IOThread));
            _ioThread.Start();
            Thread.Sleep(0);
        }

        /// <summary>
        /// Accept Thread가 종료할 때 까지 대기합니다.
        /// </summary>
        /// <exception cref="ThreadStateException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        protected void JoinAcceptThread()
        {
            if (_acceptThread != null)
                _acceptThread.Join();
        }

        /// <summary>
        /// IO Thread가 종료할 때 까지 대기합니다.
        /// </summary>
        /// <exception cref="ThreadStateException"></exception>
        /// <exception cref="ThreadInterruptedException"></exception>
        protected void JoinIOThread()
        {
            if (_ioThread != null)
                _ioThread.Join();
        }

    }
}

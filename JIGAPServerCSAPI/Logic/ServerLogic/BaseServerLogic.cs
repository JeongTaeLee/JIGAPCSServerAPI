﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace JIGAPServerCSAPI.Logic
{
    public abstract class BaseServerLogic
    {
        protected BaseProcessLogic _processLogic = null;
        protected BaseProcessLogic processLogic { get => _processLogic; }

        protected Thread _acceptThread = null;
        protected Thread _ioThread = null;

        protected Action<string> _logPrinter = null;

        protected bool _isServerOn = false;
        public bool isServerOn { get => _isServerOn; }

        protected int _userMaxCount = 100000;

        protected int _packetMaxSize = 2048;
        

        public BaseServerLogic(BaseProcessLogic inProcessLogic)
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

            _acceptThread = null;
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

            _ioThread = null;
        }

        protected void PrintLog(string inLog)
        {
            if (string.IsNullOrEmpty(inLog))
                throw new ArgumentException("Param inLog is Empty string and NULL");

            if (_logPrinter != null)
                _logPrinter(inLog);
        }

        /// <summary>
        /// 로그를 출력 할 수 있는 함수 포인터 변수를 셋팅합니다.
        /// </summary>
        /// <param name="inLogPrinter"></param>
        public void SetLogPrinter(Action<string> inLogPrinter)
        {
            if (inLogPrinter == null)
                throw new ArgumentException("Param inLogPrinter is NULL");

            _logPrinter = inLogPrinter;

            _processLogic.SetLogPrinter(_logPrinter);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class ServerInformation
    {
        private string _ipAddress = null;
        public string ipAddress { get => _ipAddress; }

        private short _portNumber = 0;
        public short portNumber { get => _portNumber; }

        private int _blockingCount = 0;
        public  int blockingCount { get => _blockingCount; }


        public void SetIpAddress(string inIpAddress)
        {
            _ipAddress = inIpAddress;
        }

        public void SetPortNumber(short inPortNumber)
        {
            _portNumber = inPortNumber;
        }

        public void SetBlockingCount(int inBlockingCount)
        {
            _blockingCount = inBlockingCount;
        }
    }
}

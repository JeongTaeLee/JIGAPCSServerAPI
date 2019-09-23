using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace JIGAPServerCSAPI
{
    public abstract class BaseSocket
    {
        public abstract void StartSocket(string inIpAddress, int inPort, int inBlockingCount);
        public abstract void CloseSocket();
        public abstract void Accept(BaseSocket inSocket);
        public abstract void Connect(string inIpAddress, int inPort);
    }
   
}

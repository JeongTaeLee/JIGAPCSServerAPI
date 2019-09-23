using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket socket = new JIGAPServerCSAPI.AsyncEventAPI.AsyncEventSocket();
                socket.Connect("127.0.0.1", 9199);
            }
        }
    }
}

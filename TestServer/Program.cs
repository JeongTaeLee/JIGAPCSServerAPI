using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            JIGAPServerCSAPI.AsyncEventAPI.AsyncEventServerLogic serverLogic = new JIGAPServerCSAPI.AsyncEventAPI.AsyncEventServerLogic(new TestProcessLogic());
            serverLogic.SetLogPrinter(Console.WriteLine);

            serverLogic.StartServer("127.0.0.1", 9199, 50);
            
            while (true)
            {
                // 명령문을 추가 해야함.
            }

            // 아직 여기 안들어옴
            serverLogic.EndServer();
            serverLogic.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static JIGAPServerCSAPI.AsyncEventAPI.AsyncEventServerLogic serverLogic = null;
        static ServerInformation _serverInfomation = new ServerInformation(); 


        static void ServerCommand(string[] inStr)
        {
            if (inStr[0] != "server")
            {
                PrintLog("Command is Invalid");
                return;
            }

            if (inStr.Length < 2)
            {
                PrintLog("Error! Command(server) need 3 param");
                return;
            }

            switch (inStr[1])
            {
                case "open":
                    if (serverLogic.isServerOn)
                    {
                        PrintLog("Server is open");
                        return;
                    }

                    serverLogic.SetLogPrinter(PrintLog);
                    serverLogic.StartServer(_serverInfomation.ipAddress, _serverInfomation.portNumber, _serverInfomation.blockingCount);

                    break;

                case "close":
                    if (serverLogic.isServerOn == false)
                    {
                        PrintLog("Server is not open");
                        return;
                    }

                    serverLogic.EndServer();
                    serverLogic = null;

                    GC.Collect();
                    break;
            }

            
        }
        static void SettingCommand(string[] inStr)
        {
            if (inStr[0] != "setting")
            {
                PrintLog("Command is Invalid");
                return;
            }

            if (inStr.Length < 3)
            {
                PrintLog("Error! Command(setting) need 3 param");
                return;
            }

            switch (inStr[1])
            {
                case "ip":
                    _serverInfomation.SetIpAddress(inStr[2]);
                    PrintLog($"Change the server ip {inStr[2]}");
                    break;
                case "port":
                    _serverInfomation.SetIpAddress(inStr[2]);
                    PrintLog($"Change the server port {inStr[2]}");
                    break;
                case "blocking":
                    _serverInfomation.SetIpAddress(inStr[2]);
                    PrintLog($"Change the server blocking{inStr[2]}");
                    break;
                default:
                    PrintLog("Error! Command(setting) need 3 param");
                    break;
            }

        }

        static void PrintLog(string str)
        {
            Console.WriteLine($"$SYSTEM : { str }");
        }

        static void Main(string[] args)
        {
            serverLogic = new JIGAPServerCSAPI.AsyncEventAPI.AsyncEventServerLogic(new TestProcessLogic());

            _serverInfomation.SetIpAddress("127.0.0.1");
            _serverInfomation.SetPortNumber(9199);
            _serverInfomation.SetBlockingCount(50);

            CommandInputer inputer = new CommandInputer((string[] str) => { Console.WriteLine("Command is Invalid."); });

            //inputer.AddCommand("start_server", (string[] str) => { StartServer(); });
            inputer.AddCommand("server", ServerCommand);
            inputer.AddCommand("setting", SettingCommand);

            inputer.RunCommand();

        }
    }
}

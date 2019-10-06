using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class CommandInputer
    {
        public delegate void CommandFunc(string[] inFunc);

        private Hashtable _commandTable = new Hashtable();
        private CommandFunc _defaultFunc = null;

        public CommandInputer(CommandFunc inDefaultCmdFunc)
        {
            if (inDefaultCmdFunc == null)
                throw new ArgumentException("Param inDDefaultCmdFunc is NULL");

            _commandTable.Add("Default", inDefaultCmdFunc);
            _defaultFunc = inDefaultCmdFunc;
        }

        public void AddCommand(string inCmdStr, CommandFunc inCmdFunc)
        {
            if (string.IsNullOrEmpty(inCmdStr))
                throw new ArgumentException("Param inDDefaultCmdFunc is NULL or Empty");

            if (inCmdStr == null)
                throw new ArgumentException("Param inDDefaultCmdFunc is NULL");

            if (_commandTable.ContainsKey(inCmdStr))
                return;

            _commandTable.Add(inCmdStr, inCmdFunc);
        }

        public void InputCommand(string inCmdStr)
        {
            if (string.IsNullOrEmpty(inCmdStr))
                Console.Write("Please Input Comment");

            string[] splitStr = inCmdStr.Split(' ');

            if (!_commandTable.ContainsKey(splitStr[0]))
            {
                if (_defaultFunc != null)
                {
                    _defaultFunc(null);
                    return;
                }
            }

            CommandFunc func = (CommandFunc)_commandTable[splitStr[0]];
            func(splitStr);
        }

        public void RunCommand()
        {
            while (true)
            {
                string str = Console.ReadLine();
                InputCommand(str);
            }
        }
    }
}

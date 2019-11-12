using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIGAPServerCSAPI
{
    static class Loger
    {
        private static Action<string[]> logPrinter = null;

        public static void Initialzie(Action<string[]> inLogPrinter)
        {
            logPrinter = inLogPrinter;
        }

        public static void Log(params string[] log)
        {
            logPrinter?.Invoke(log);
        }
    }
}

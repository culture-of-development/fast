using System;

namespace fast.webapi.Controllers
{
    public static class Log
    {
        public static void WriteLine(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}
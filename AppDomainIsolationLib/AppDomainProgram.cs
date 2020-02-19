using System;

namespace AppDomainIsolationLib
{
    public class AppDomainProgram
    {
        public static int InitCount = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine($"Init Count: {InitCount} (CurrentDomain: {AppDomain.CurrentDomain.FriendlyName})");
        }
    }
}

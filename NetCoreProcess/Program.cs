using System;
using System.Diagnostics;
using TaskLib;

namespace NetCoreProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            //byte[] x = new byte[512 * 1024 * 1024]; // allocate 1GB memory
            //(new Random()).NextBytes(x);

            TaskLib.Program.ProcessMain(args);
        }

    }
}

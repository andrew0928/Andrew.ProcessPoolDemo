using System;
using System.Diagnostics;
using TaskLib;

namespace NetCoreWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            string mode = args[0]; // "VALUE"; // VALUE | BASE64
            TaskLib.Program.WorkerMain(mode, new HelloWorkerBase[] {
                new InProcessWorker(),
                //new SingleAppDomainWorker(),
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe", mode),
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetCoreProcess\bin\Debug\netcoreapp3.1\NetCoreProcess.exe", mode)
            });
        }
    }
}

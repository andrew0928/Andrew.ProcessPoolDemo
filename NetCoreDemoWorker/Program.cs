using System;
using System.Threading.Tasks;
using TaskLib;

namespace NetCoreDemoWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new ProcessPoolWorker(
                @"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe",
                2, 5, 3000);

            for (int i = 0; i < 100; i++) worker.QueueTask(new byte[1 * 1024 * 1024]);

            Console.WriteLine("Take a rest (worker idle 10 sec)...");
            Task.Delay(10 * 1000).Wait();
            Console.WriteLine("Wake up, start work.");

            for (int i = 0; i < 100; i++) worker.QueueTask(new byte[1024 * 1024]);
            worker.Stop();
        }
    }
}

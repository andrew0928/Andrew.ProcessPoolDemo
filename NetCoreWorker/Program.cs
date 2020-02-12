using System;
using System.Diagnostics;
using TaskLib;

namespace NetCoreWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            //var worker =
            //new InProcessWorker();
            //new SingleAppDomainWorker();
            //new SingleProcessWorker();

            Stopwatch _timer = new Stopwatch();
            int count = 1000;

            foreach (var worker in (new HelloWorkerBase[] {
                new InProcessWorker(), 
                //new SingleAppDomainWorker(), 
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe"),
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetCoreProcess\bin\Debug\netcoreapp3.1\NetCoreProcess.exe"),}))
            {
                _timer.Restart();
                for (int i = 0; i < count; i++)
                {
                    var task = worker.QueueTask(1);

                    //task.Wait.Wait();
                    //Console.WriteLine(task.ReturnValue);
                }
                worker.Stop();

                Console.WriteLine($"{worker.GetType().Name}: {count * 1000.0 / _timer.ElapsedMilliseconds} tasks/sec");
            }
        }
    }
}

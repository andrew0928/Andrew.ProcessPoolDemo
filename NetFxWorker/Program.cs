using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskLib;

namespace NetFxWorker
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
                new SingleAppDomainWorker(),
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













    public class SingleAppDomainWorker : HelloWorkerBase
    {
        private AppDomain _domain = null;

        public SingleAppDomainWorker()
        {
            this._domain = AppDomain.CreateDomain("demo");
        }

        public override HelloTaskResult QueueTask(int size)
        {
            HelloTask ht = this._domain.CreateInstanceAndUnwrap(typeof(HelloTask).Assembly.FullName, typeof(HelloTask).FullName) as HelloTask;

            return new HelloWorkerBase.HelloTaskResult(true)
            {
                //ReturnValue = ht.DoTask(size),
                ReturnValue = ht.DoTask(new byte[1 * 1024 * 1024]),
            };
        }

        public override void Stop()
        {
            AppDomain.Unload(this._domain);
            this._domain = null;
            return;
        }
    }
}

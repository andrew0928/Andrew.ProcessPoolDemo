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
            var worker =
                //new InProcessWorker();
                //new ThreadWorker();
                new AppDomainWorker();

            Stopwatch _timer = new Stopwatch();
            int count = 100;

            _timer.Restart();
            for (int i = 0;i<count; i++)
            {
                var task = worker.QueueTask(1);
                task.Wait.Wait();
                Console.WriteLine(task.Result);
            }
            worker.CompleteWorker();

            Console.WriteLine($"{worker.GetType().Name}: {count * 1000.0 / _timer.ElapsedMilliseconds} tasks/sec");
        }
    }













    public class AppDomainWorker : WorkerBase
    {
        private AppDomain _domain = null;

        public AppDomainWorker()
        {
            this._domain = AppDomain.CreateDomain("demo");
        }

        public override TaskWrap<int, string> QueueTask(int size)
        {
            HelloTask ht = this._domain.CreateInstanceAndUnwrap(typeof(HelloTask).Assembly.FullName, typeof(HelloTask).FullName) as HelloTask;

            TaskWrap<int, string> tw = new WorkerBase.TaskWrap<int, string>()
            {
                Args = size,
                //Result = ht.DoTask(size),
                Result = Convert.ToBase64String(ht.DoTask(new byte[1024 * 1024])),
            };
            
            tw.Wait.Set();
            return tw;
        }

        public override void CompleteWorker()
        {
            return;
        }
    }
}

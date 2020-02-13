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
            string mode = args[0]; // "VALUE"; // VALUE | BASE64
            TaskLib.Program.WorkerMain(mode, new HelloWorkerBase[] 
            {
                new InProcessWorker(),
                new SingleAppDomainWorker(),
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe", mode),
                new SingleProcessWorker(@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetCoreProcess\bin\Debug\netcoreapp3.1\NetCoreProcess.exe", mode)
            });
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
                ReturnValue = ht.DoTask(size),
            };
        }
        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            HelloTask ht = this._domain.CreateInstanceAndUnwrap(typeof(HelloTask).Assembly.FullName, typeof(HelloTask).FullName) as HelloTask;

            return new HelloWorkerBase.HelloTaskResult(true)
            {
                ReturnValue = ht.DoTask(buffer),
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

using HelloWorldTask;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppDomainWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            int total_domain_cycle_count = 1;

            //if (false)
            {
                timer.Restart();
                for (int i = 0; i < total_domain_cycle_count; i++)
                {
                    HelloWorldTask.Program.Main(null);
                }
                Console.WriteLine($"Benchmark (InProcess Mode):           { total_domain_cycle_count * 1000.0 / timer.ElapsedMilliseconds } run / sec");
            }

            //if (false)
            {
                timer.Restart();
                for (int i = 0; i < total_domain_cycle_count; i++)
                {
                    Thread t = new Thread(() =>
                    {
                        HelloWorldTask.Program.Main(null);
                    });
                    t.Start();
                    t.Join();
                }
                Console.WriteLine($"Benchmark (Thread Mode):              { total_domain_cycle_count * 1000.0 / timer.ElapsedMilliseconds } run / sec");
            }

            if (false)
            {
                timer.Restart();
                for (int i = 0; i < total_domain_cycle_count; i++)
                {
                    var d = AppDomain.CreateDomain("demo");
                    var r = d.ExecuteAssemblyByName(typeof(HelloWorld).Assembly.FullName);
                    AppDomain.Unload(d);
                }
                Console.WriteLine($"Benchmark (AppDomain Mode):           { total_domain_cycle_count * 1000.0 / timer.ElapsedMilliseconds } run / sec");
            }

            //if (false)
            {
                timer.Restart();
                for (int i = 0; i < total_domain_cycle_count; i++)
                {
                    var p = Process.Start(new ProcessStartInfo()
                    {
                        FileName = @"C:\CodeWork\github.com\Andrew.ProcessPoolDemo\ProcessHostFx\bin\Release\ProcessHostFx.exe",              // .net fx 4.7.2
                        WindowStyle = ProcessWindowStyle.Hidden,
                    });
                    p.WaitForExit();
                }
                Console.WriteLine($"Benchmark (.NET Fx Process Mode):     { total_domain_cycle_count * 1000.0 / timer.ElapsedMilliseconds } run / sec");
            }

            //if (false)
            {
                timer.Restart();
                for (int i = 0; i < total_domain_cycle_count; i++)
                {
                    var p = Process.Start(new ProcessStartInfo()
                    {
                        FileName = @"C:\CodeWork\github.com\Andrew.ProcessPoolDemo\ProcessHost\bin\Release\netcoreapp3.1\ProcessHost.exe",    // .net core 3.1
                        WindowStyle = ProcessWindowStyle.Hidden,
                    });
                    p.WaitForExit();
                }
                Console.WriteLine($"Benchmark (.NET Core Process Mode):   { total_domain_cycle_count * 1000.0 / timer.ElapsedMilliseconds } run / sec");
            }
        }
    }


    public class AppDomainPool
    {
        private BlockingCollection<string> _queue = null;
        private int _pool_size = 10;

        private Dictionary<string, Thread> _pools = null;

        public AppDomainPool()
        {
            this._queue = new BlockingCollection<string>();
            this._pools = new Dictionary<string, Thread>();

            for (int i = 0; i < this._pool_size; i++)
            {

            }
        }

        private void AppDomainTaskHandler(string id)
        {
            var domain = AppDomain.CreateDomain($"domain#{id}");
            //domain.ExecuteAssemblyByName("", )
        }

        public void QueueTask(string name)
        {
            this._queue.Add(name);
        }

        public void Stop()
        {
            this._queue.CompleteAdding();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;

namespace TaskLib
{
    public abstract class HelloWorkerBase
    {
        public HelloWorkerBase()
        {
        }

        public abstract HelloTaskResult QueueTask(int size);

        public abstract void Stop();

        
        public class HelloTaskResult
        {
            public HelloTaskResult(bool waitState = false)
            {
                this.Wait = new ManualResetEventSlim(waitState);
            }
            //public string ReturnValue;
            public byte[] ReturnValue;
            public readonly ManualResetEventSlim Wait;
        }

    }


    public class InProcessWorker : HelloWorkerBase
    {
        public override HelloTaskResult QueueTask(int size)
        {
            return new HelloWorkerBase.HelloTaskResult(true)
            {
                //ReturnValue = (new HelloTask()).DoTask(size)
                ReturnValue = (new HelloTask()).DoTask(new byte[size * 1024 * 1024]),
            };
        }

        public override void Stop()
        {
            return;
        }
    }

    public class ThreadWorker : HelloWorkerBase
    {
        private Thread _worker_thread = null;
        private BlockingCollection<(int size, HelloTaskResult result)> _queue;

        public ThreadWorker() : base()
        {
            this._queue = new BlockingCollection<(int size, HelloTaskResult result)>();

            this._worker_thread = new Thread(this.ThreadHandler);
            this._worker_thread.Start();
        }

        private void ThreadHandler()
        {
            //while(this._queue.TryTake(out TaskWrap<int, string> task) == true)
            while(true)
            {
                try
                {
                    var task = this._queue.Take();
                    //task.result.ReturnValue = (new HelloTask()).DoTask(task.size);
                    task.result.ReturnValue = (new HelloTask()).DoTask(new byte[task.size * 1024 * 1024]);
                    task.result.Wait.Set();
                }
                catch(InvalidOperationException)
                {
                    break;
                }
            }
        }

        public override HelloTaskResult QueueTask(int size)
        {
            HelloTaskResult result = new HelloTaskResult();
            this._queue.Add((size, result));
            return result;
        }

        public override void Stop()
        {
            this._queue.CompleteAdding();
            this._worker_thread.Join();
            return;
        }
    }

    public class SingleProcessWorker : HelloWorkerBase
    {
        private Process _process = null;
        private TextReader _reader = null;
        private TextWriter _writer = null;

        public SingleProcessWorker(string path)
        {
            this._process = Process.Start(new ProcessStartInfo()
            {
                FileName = path, //@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe",
                Arguments = "",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            this._reader = this._process.StandardOutput;
            this._writer = this._process.StandardInput;
        }

        public override HelloTaskResult QueueTask(int size)
        {
            //this._writer.WriteLine(size);
            this._writer.WriteLine(Convert.ToBase64String(new byte[size * 1024 * 1024]));

            return new HelloTaskResult(true)
            {
                //ReturnValue = this._reader.ReadLine()
                ReturnValue = Convert.FromBase64String(this._reader.ReadLine())
            };
        }

        public override void Stop()
        {
            this._writer.Close();
            this._process.WaitForExit();
        }
    }



    /*
    public class AppDomainWorker : WorkerBase
    {
        private Thread _worker_thread = null;
        private Thread _domain_thread = null;
        private NamedPipeClientStream _pipeClient = null;

        private BlockingCollection<TaskWrap<int, string>> _queue;

        public AppDomainWorker() : base()
        {
            this._queue = new BlockingCollection<TaskWrap<int, string>>();
            this._pipeClient = new NamedPipeClientStream("demo-args");

            this._domain_thread = new Thread(() =>
            {
                var domain = AppDomain.CreateDomain("demo");
                domain.ExecuteAssemblyByName(typeof(Program).Assembly.FullName, "demo-args");
                AppDomain.Unload(domain);
            });
            this._domain_thread.Start();

            this._worker_thread = new Thread(this.AppDomainHandler);
            this._worker_thread.Start();
        }

        private void AppDomainHandler()
        {
            BinaryWriter bw = new BinaryWriter(this._pipeClient);
            while (true)
            {
                try
                {
                    var task = this._queue.Take();
                    bw.Write(task.Args);
                    //task.Result = (new HelloTask()).DoTask(task.Args);
                    task.Wait.Set();
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

        public override TaskWrap<int, string> QueueTask(int size)
        {
            TaskWrap<int, string> task = new TaskWrap<int, string>()
            {
                Args = size
            };
            this._queue.Add(task);
            return task;
        }

        public override void CompleteWorker()
        {
            this._queue.CompleteAdding();

            this._worker_thread.Join();
            this._domain_thread.Join();
            return;
        }
    }
    */
}

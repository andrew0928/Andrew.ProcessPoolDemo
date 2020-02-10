using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.IO;

namespace TaskLib
{
    public abstract class WorkerBase
    {
        public WorkerBase()
        {
        }

        public abstract TaskWrap<int,string> QueueTask(int size);

        public abstract void CompleteWorker();

        public class TaskWrap<TArgs, TResult>
        {
            public TArgs Args;
            public TResult Result;
            public ManualResetEventSlim Wait = new ManualResetEventSlim(false);
        }
    }


    public class InProcessWorker : WorkerBase
    {
        public override TaskWrap<int, string> QueueTask(int size)
        {
            TaskWrap<int, string> tw = new WorkerBase.TaskWrap<int, string>()
            {
                Args = size,
                Result = (new HelloTask()).DoTask(size),
            };
            tw.Wait.Set();
            return tw;
        }

        public override void CompleteWorker()
        {
            return;
        }
    }



    public class ThreadWorker : WorkerBase
    {
        private Thread _worker_thread = null;
        private BlockingCollection<TaskWrap<int, string>> _queue;

        public ThreadWorker() : base()
        {
            this._queue = new BlockingCollection<TaskWrap<int, string>>();

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
                    task.Result = (new HelloTask()).DoTask(task.Args);
                    task.Wait.Set();
                }
                catch(InvalidOperationException)
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
            return;
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

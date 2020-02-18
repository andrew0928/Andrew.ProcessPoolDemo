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
        public abstract HelloTaskResult QueueTask(byte[] buffer);

        public abstract void Stop();

        
        public class HelloTaskResult
        {
            public HelloTaskResult(bool waitState = false)
            {
                this.Wait = new ManualResetEventSlim(waitState);
            }
            public string ReturnValue;
            public readonly ManualResetEventSlim Wait;
        }

    }


    /*
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

#if MODE_BUFFER
                    task.result.ReturnValue = (new HelloTask()).DoTask(new byte[task.size * 1024 * 1024]);
#else
                    task.result.ReturnValue = (new HelloTask()).DoTask(task.size);
#endif

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
    */






}

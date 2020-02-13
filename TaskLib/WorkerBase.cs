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


    public class InProcessWorker : HelloWorkerBase
    {
        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            return new HelloWorkerBase.HelloTaskResult(true)
            {
                ReturnValue = (new HelloTask()).DoTask(buffer),
            };
        }
        public override HelloTaskResult QueueTask(int size)
        {
            return new HelloWorkerBase.HelloTaskResult(true)
            {
                ReturnValue = (new HelloTask()).DoTask(size)
            };
        }

        public override void Stop()
        {
            return;
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

    public class SingleProcessWorker : HelloWorkerBase
    {
        private Process _process = null;
        private TextReader _reader = null;
        private TextWriter _writer = null;
        private string _mode = null;

        public SingleProcessWorker(string filename, string args)
        {
            this._mode = args;
            this._process = Process.Start(new ProcessStartInfo()
            {
                FileName = filename, //@"D:\CodeWork\github.com\Andrew.ProcessPoolDemo\NetFxProcess\bin\Debug\NetFxProcess.exe",
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            this._reader = this._process.StandardOutput;
            this._writer = this._process.StandardInput;
        }

        public override HelloTaskResult QueueTask(int size)
        {
            if (this._mode != "VALUE") throw new InvalidOperationException();
            this._writer.WriteLine(size);
            return new HelloTaskResult(true)
            {
                ReturnValue = this._reader.ReadLine()
            };
        }

        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            if (this._mode != "BASE64") throw new InvalidOperationException();
            this._writer.WriteLine(Convert.ToBase64String(buffer));
            return new HelloTaskResult(true)
            {
                ReturnValue = this._reader.ReadLine()
            };
        }

        public override void Stop()
        {
            this._writer.Close();
            this._process.WaitForExit();
        }
    }

}

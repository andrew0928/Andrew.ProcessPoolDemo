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




    public class ProcessPoolWorker : HelloWorkerBase
    {
        // pool settings
        private readonly int _buffer_size = 30;
        private readonly int _min_pool_size = 1;
        private readonly int _max_pool_size = 24;
        private readonly TimeSpan _process_idle_timeout = TimeSpan.FromSeconds(5);

        private readonly string _filename = null;
        private BlockingCollection<(byte[] buffer, HelloTaskResult result)> _queue = null;
        private List<Thread> _threads = null;
        private object _syncroot = new object();
        private int _total_working_process_count = 0;
        private AutoResetEvent _wait = new AutoResetEvent(false);

        public ProcessPoolWorker(string filename)
        {
            this._filename = filename;
            this._queue = new BlockingCollection<(byte[] buffer, HelloTaskResult result)>(this._buffer_size);
            this._threads = new List<Thread>();
        }

        private void AdjustPools()
        {
            lock (this._syncroot)
            {
                if (this._threads.Count >= this._max_pool_size) return;
                if (this._threads.Count > this._total_working_process_count) return;

                var t = new Thread(this.ProcessHandler);
                this._threads.Add(t);
                t.Start();
            }
        }

        private void ProcessHandler()
        {
            var _process = Process.Start(new ProcessStartInfo()
            {
                FileName = this._filename, 
                Arguments = "BASE64",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            _process.PriorityClass = ProcessPriorityClass.BelowNormal;
            //_process.ProcessorAffinity = new IntPtr(14);    // 0000 0000 0000 1110

            var _reader = _process.StandardOutput;
            var _writer = _process.StandardInput;

            //Console.WriteLine($"* Process [PID: {_process.Id}] Started.");
            do
            {
                if (this._queue.TryTake(out var item, this._process_idle_timeout) == true)
                {
                    this.AdjustPools();
                    lock (this._syncroot) this._total_working_process_count++;
                    _writer.WriteLine(Convert.ToBase64String(item.buffer));
                    item.result.ReturnValue = _reader.ReadLine();
                    item.result.Wait.Set();
                    lock (this._syncroot) this._total_working_process_count--;
                }
            } while (this._queue.IsCompleted == false || this._threads.Count <= this._min_pool_size);

            _writer.Close();
            _process.WaitForExit();
            this._wait.Set();
            //Console.WriteLine($"* Process [PID: {_process.Id}] Stopped.");

        }

        public override HelloTaskResult QueueTask(int size)
        {
            throw new NotSupportedException();
        }

        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            HelloTaskResult result = new HelloTaskResult();
            this._queue.Add((buffer, result));
            this.AdjustPools();
            return result;
        }

        public override void Stop()
        {
            this._queue.CompleteAdding();
            while(this._queue.IsCompleted == false)
            {
                this._wait.WaitOne();
            }
        }
    }

}

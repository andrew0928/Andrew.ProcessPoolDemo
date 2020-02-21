using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TaskLib
{
    public class ProcessPoolWorker : HelloWorkerBase
    {
        // pool settings
        private readonly int _min_pool_size = 0;
        private readonly int _max_pool_size = 0;
        private readonly TimeSpan _process_idle_timeout = TimeSpan.Zero;

        // pool states
        private readonly string _filename = null;
        private BlockingCollection<(byte[] buffer, HelloTaskResult result)> _queue = new BlockingCollection<(byte[] buffer, HelloTaskResult result)>(5);    // buffer size
        private List<Thread> _threads = new List<Thread>();
        private object _syncroot = new object();
        private int _total_working_process_count = 0;
        private int _total_created_process_count = 0;
        private AutoResetEvent _wait = new AutoResetEvent(false);

        public ProcessPoolWorker(string filename, int processMin = 1, int processMax = 20, int processIdleTimeoutMilliseconds = 10000)
        {
            this._filename = filename;
            this._min_pool_size = processMin;
            this._max_pool_size = processMax;
            this._process_idle_timeout = TimeSpan.FromMilliseconds(processIdleTimeoutMilliseconds);
        }

        private bool TryIncreaseProcess()
        {
            lock (this._syncroot)
            {
                if (this._total_created_process_count >= this._max_pool_size) return false;
                if (this._total_created_process_count > this._total_working_process_count) return false;
                if (this._queue.Count == 0) return false;
                if (this._queue.IsCompleted) return false;
            }

            var t = new Thread(this.ProcessHandler);
            this._threads.Add(t);
            t.Start();
            return true;
        }
        private bool ShouldDecreaseProcess()
        {
            lock (this._syncroot)
            {
                if (this._total_created_process_count <= this._min_pool_size) return false;
                if (this._queue.Count > 0) return false;
            }
            return true;
        }

        private void ProcessHandler()
        {
            lock (this._syncroot) this._total_created_process_count++;
            var _process = Process.Start(new ProcessStartInfo(this._filename, "BASE64")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            _process.PriorityClass = ProcessPriorityClass.BelowNormal;
            //_process.ProcessorAffinity = new IntPtr(14);    // 0000 0000 0000 1110

            var _reader = _process.StandardOutput;
            var _writer = _process.StandardInput;

            Console.WriteLine($"* {DateTime.Now} - Process [PID: {_process.Id}] Started.");
            while (this._queue.IsCompleted == false)
            {
                if (this._queue.TryTake(out var item, this._process_idle_timeout) == false)
                {
                    if (this.ShouldDecreaseProcess())
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"* {DateTime.Now} - Process [PID: {_process.Id}] Keep alive for this process.");
                        continue;
                    }
                }

                this.TryIncreaseProcess();
                lock (this._syncroot) this._total_working_process_count++;
                _writer.WriteLine(Convert.ToBase64String(item.buffer));
                item.result.ReturnValue = _reader.ReadLine();
                item.result.Wait.Set();
                lock (this._syncroot) this._total_working_process_count--;
            }
            lock (this._syncroot) this._total_created_process_count--;
            Console.WriteLine($"* {DateTime.Now} - Process [PID: {_process.Id}] Stopped.");

            _writer.Close();
            _process.WaitForExit();
            this._wait.Set();
        }

        public override HelloTaskResult QueueTask(int size)
        {
            throw new NotSupportedException();
        }

        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            HelloTaskResult result = new HelloTaskResult();
            this._queue.Add((buffer, result));
            this.TryIncreaseProcess();
            return result;
        }

        public override void Stop()
        {
            this._queue.CompleteAdding();
            while (this._queue.IsCompleted == false) this._wait.WaitOne();
        }
    }

}

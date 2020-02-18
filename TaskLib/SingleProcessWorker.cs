using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TaskLib
{
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

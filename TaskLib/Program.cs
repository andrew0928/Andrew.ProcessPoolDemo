using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;

namespace TaskLib
{
    public class Program
    {
        public static void WorkerMain(string mode, HelloWorkerBase[] workers)
        {
            Stopwatch _timer = new Stopwatch();
            int count = 10000;
            int buffer_size = 1 * 1024; // 1kb

            Console.WriteLine();
            Console.WriteLine($"Worker: {System.Reflection.Assembly.GetEntryAssembly().GetName().Name}, Mode: {mode}");
            foreach (var worker in workers)
            {
                _timer.Restart();
                for (int i = 0; i < count; i++)
                {
                    HelloWorkerBase.HelloTaskResult result = null;

                    if (mode == "VALUE")  result = worker.QueueTask(buffer_size);
                    if (mode == "BASE64") result = worker.QueueTask(new byte[buffer_size]);

                    //result.Wait.Wait();
                    //Console.WriteLine(result.ReturnValue);
                }
                worker.Stop();

                Console.WriteLine($"{worker.GetType().Name.PadRight(30)}: {count * 1000.0 / _timer.ElapsedMilliseconds} tasks/sec");
            }
        }

        public static void ProcessMain(string[] args)
        {
            string line = null;
            string mode = "VALUE";

            if (args != null & args.Length > 0) mode = args[0];      // transfer mode: VALUE | BASE64

            switch (mode)
            {
                case "VALUE":
                    while ((line = Console.ReadLine()) != null)
                    {
                        int size = int.Parse(line);
                        Console.WriteLine((new HelloTask()).DoTask(size));
                    }
                    break;

                //case "BINARY":
                //    break;

                case "BASE64":
                    while ((line = Console.ReadLine()) != null)
                    {
                        byte[] buffer = Convert.FromBase64String(line);
                        Console.WriteLine((new HelloTask()).DoTask(buffer));
                    }
                    break;
            }
        }

    }
}

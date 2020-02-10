using HelloWorldTask;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessHost
{
    class Program
    {
        public static int Main(string[] args)
        {
            //return HelloWorldTask.Program.Main(args);

            for (int i = 1; i < 100; i++)
            {
                new HelloWorld().DoTask();
            }
            //System.Threading.Thread.Sleep(10 * 1000);
            return 0;
        }
    }
}

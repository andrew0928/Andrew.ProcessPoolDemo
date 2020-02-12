using HelloWorldTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessHostFx
{
    class Program
    {
        static void Main(string[] args)
        {
            //return;

            for (int i = 1; i < 100; i++)
            {
                new HelloWorld().DoTask();
            }
            //System.Threading.Thread.Sleep(10 * 1000);
        }
    }
}

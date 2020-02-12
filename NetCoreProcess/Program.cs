﻿using System;
using System.Diagnostics;
using TaskLib;

namespace NetCoreProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            string line = null;

            while ((line = Console.ReadLine()) != null)
            {
                //int size = int.Parse(line);
                //var result = (new HelloTask()).DoTask(size);
                //Console.WriteLine(result);

                byte[] buffer = Convert.FromBase64String(line);
                var hash = (new HelloTask()).DoTask(buffer);
                Console.WriteLine(Convert.ToBase64String(hash));
            }

        }

    }
}

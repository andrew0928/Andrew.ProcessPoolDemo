using System;
using System.Security.Cryptography;

namespace HelloWorldTask
{
    public class HelloWorld
    {
        //private static int _count = 0;
        public string DoTask()
        {
            //_count++;
            //var result = $"Hello world, {name} !";
            ////Console.Error.WriteLine(result);
            //return result;

            Random rnd = new Random();
            byte[] buffer = new byte[16 * 1024 * 1024]; // 16mb
            rnd.NextBytes(buffer);

            var ha = HashAlgorithm.Create("SHA512");
            var hash = ha.ComputeHash(buffer);
            //Console.Write(".");

            return Convert.ToBase64String(hash);
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading;

namespace TaskLib
{
    public class HelloTask : MarshalByRefObject
    {
        public static int _count = 0;
        private static Random _rnd = new Random();
        private static HashAlgorithm _ha = HashAlgorithm.Create("SHA512");

        public string DoTask(int bufferSize)
        {
            byte[] buffer = new byte[bufferSize * 1024 * 1024];
            var hash = this.DoTask(buffer);
            return $"[{_count}]" +　Convert.ToBase64String(hash);
        }

        public byte[] DoTask(byte[] buffer)
        {
            _rnd.NextBytes(buffer);
            return _ha.ComputeHash(buffer);
        }
    }

}

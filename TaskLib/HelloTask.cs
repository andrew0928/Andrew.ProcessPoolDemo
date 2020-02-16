//#define NULL_LOAD

using System;
using System.Security.Cryptography;
using System.Threading;

namespace TaskLib
{
    public class HelloTask : MarshalByRefObject
    {
        public static bool state_must_be_true = true;

#if NULL_LOAD
        public string DoTask(int size) => null;
        public string DoTask(byte[] buffer) => ""; //Convert.ToBase64String(new byte[512]);
#else


        private static Random _rnd = new Random();
        private static HashAlgorithm _ha = HashAlgorithm.Create("SHA512");

        public string DoTask(int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            return this.DoTask(buffer);
        }

        public string DoTask(byte[] buffer)
        {
            if (state_must_be_true == false) throw new InvalidProgramException();

            _rnd.NextBytes(buffer);
            return Convert.ToBase64String(_ha.ComputeHash(buffer));
        }

#endif
    }

}

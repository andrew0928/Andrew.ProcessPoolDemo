using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.IO;

namespace TaskLib
{
    class Program
    {
        static void Main(string[] args)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(args[0]);
            BinaryReader br = new BinaryReader(pipeServer);

            while(pipeServer.CanRead)
            {
                string result = (new HelloTask()).DoTask(br.ReadInt32());
            }
            
        }
    }
}

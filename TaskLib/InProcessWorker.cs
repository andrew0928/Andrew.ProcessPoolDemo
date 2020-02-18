using System;
using System.Collections.Generic;
using System.Text;

namespace TaskLib
{
    public class InProcessWorker : HelloWorkerBase
    {
        public override HelloTaskResult QueueTask(byte[] buffer)
        {
            return new HelloWorkerBase.HelloTaskResult(true)
            {
                ReturnValue = (new HelloTask()).DoTask(buffer),
            };
        }
        public override HelloTaskResult QueueTask(int size)
        {
            return new HelloWorkerBase.HelloTaskResult(true)
            {
                ReturnValue = (new HelloTask()).DoTask(size)
            };
        }

        public override void Stop()
        {
            return;
        }
    }

}

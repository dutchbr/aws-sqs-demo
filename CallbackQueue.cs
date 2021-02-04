using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public class CallbackQueue : IReceiveMessage
    {
        public void onMessage(string message, bool ack)
        {
            Console.WriteLine(message);
            ack = true;
        }
    }
}

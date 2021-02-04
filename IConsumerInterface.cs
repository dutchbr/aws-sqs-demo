using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public interface IConsumerInterface
    {
        void StartConsumer(string queueName,IReceiveMessage callback);
                
    }

    public interface IReceiveMessage
    {
        void onMessage(string message, bool ack);
    }

}

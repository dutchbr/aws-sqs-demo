using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public interface IProducerInterface
    {
        
        void PutMessage(string queueName, string message, bool autoCreateQueue = false);



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace aws_sqs_demo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class dispatcherController : ControllerBase
    {


        private readonly IProducerInterface _producer;

        public dispatcherController(IProducerInterface producerInterface)
        {
            _producer = producerInterface;

                      
        }


        [HttpPost]
        public bool PostMessage([FromBody] Message message)
        {
            if (message != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach (MessageAttr attr in message.Values)
                {
                    sb.Append(string.Format("{0}:{1}", attr.Key, attr.Value)).AppendLine();
                  
                }
                _producer.PutMessage(message.Tag, sb.ToString(), true);


            }
            
            return true;
        }

    }
}

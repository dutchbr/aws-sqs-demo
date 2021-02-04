using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public class Message
    {
        public string Tag { get; set; }
        public List<MessageAttr> Values { get; set; }


    }


    public class MessageAttr
    {
        public string Key { get; set; }
        public string Value { get; set; }



    }
}

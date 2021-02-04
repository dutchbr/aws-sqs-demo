using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public class SQSProducer : IProducerInterface
    {

        private readonly IConfiguration _configuration;
        private BasicAWSCredentials aWSCredentials;
        private AmazonSQSClient amazonSQSClient;

        private ConcurrentDictionary<String, String> queuesDic = new ConcurrentDictionary<string, string>();
        public SQSProducer(IConfiguration config)
        {
            _configuration = config;
            Initialize();

        }
        private void Initialize()
        {
            try
            {
                string ak = _configuration.GetSection("AWS:AcessKey").Value;
                string sk = _configuration.GetSection("AWS:SecretKey").Value;
                aWSCredentials = new BasicAWSCredentials(ak, sk);
                amazonSQSClient = new AmazonSQSClient(aWSCredentials, Amazon.RegionEndpoint.SAEast1);
            }
            catch (Exception e)
            {
                throw e;

            }

        }


        private string createQueue(string queueName)
        {
            var createQueueRequest = new CreateQueueRequest();
            try
            {

                createQueueRequest.QueueName = queueName;
                var attrs = new Dictionary<string, string>();
                attrs.Add(QueueAttributeName.VisibilityTimeout, "60");
                attrs.Add(QueueAttributeName.DelaySeconds, "90");
                attrs.Add(QueueAttributeName.MessageRetentionPeriod, "1209600");
                createQueueRequest.Attributes = attrs;
                CreateQueueResponse createQueueResponse = amazonSQSClient.CreateQueueAsync(createQueueRequest).Result;
                if (!queuesDic.TryAdd(queueName, createQueueResponse.QueueUrl))
                {
                    throw new Exception("Fail to Put Queue on Dic");
                }
                return createQueueResponse.QueueUrl;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        private string ValidateQueue(string QueueName, bool createIfNotExists)
        {
            GetQueueUrlResponse s = null;
            string result = string.Empty;
            try
            {
                GetQueueUrlRequest urlQueueRequest = new GetQueueUrlRequest
                {
                    QueueName = QueueName
                };
                try
                {
                    s = amazonSQSClient.GetQueueUrlAsync(urlQueueRequest).Result;
                }
                catch (System.AggregateException)
                {
                    //QueueNotExists
                    return createQueue(QueueName);


                }
                if (s.QueueUrl.Any())
                {
                    if (!queuesDic.TryAdd(QueueName, s.QueueUrl))
                        throw new Exception("Fail to Put Queue on Dic");
                    return s.QueueUrl;
                }
                else
                {
                    return createQueue(QueueName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void internalPutMessage(string queueName, string message, bool autoCreateQueue = false)
        {
            string queueUrl = string.Empty;

            try
            {
                if (!queuesDic.TryGetValue(queueName, out queueUrl))
                {
                    queueUrl = ValidateQueue(queueName, autoCreateQueue);

                }
                SendMessage(queueUrl, queueName, message);
            }

            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0}:{1}:{2}:{3}", "FALHA_SQS_ENVIAR", queueName, message, e.Message));
            }
        }


        public void PutMessage(string queueName, string message, bool autoCreateQueue = false)
        {
            new Task(() => { internalPutMessage(queueName, message, autoCreateQueue); }).Start();


        }

        private void SendMessage(string queueUrl, string queueName, string message)
        {
            try
            {
                SendMessageRequest sendRequest = new SendMessageRequest();
                sendRequest.QueueUrl = queueUrl;
                sendRequest.MessageBody = message;
                var sendMessageResponse = amazonSQSClient.SendMessageAsync(sendRequest).Result;
                HttpStatusCode  ret= sendMessageResponse.HttpStatusCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0}:{1}:{2}:{3}", "FALHA_SQS_ENVIAR", queueName, message, e.Message));
            }

        }

    }
}

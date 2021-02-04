using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace aws_sqs_demo
{
    public class SQSConsumer : IConsumerInterface
    {
        private readonly IConfiguration _configuration;
        private BasicAWSCredentials aWSCredentials;
        private AmazonSQSClient amazonSQSClient;
        private Thread _workerThread;
        private IReceiveMessage receiver;
        private string queueName;
        private volatile bool isRunning;
        private ReceiveMessageRequest receiveMessageRequest;
        private string _queueUrl;


        public SQSConsumer(IConfiguration config)
        {
            _configuration = config;
            Initialize();
        }

        public void StartConsumer(string queueName, IReceiveMessage callback)
        {
            this.queueName = queueName;
            this.receiver = callback;
            string queueUrl = string.Empty;
            try
            {
                queueUrl =  getQueueUrl(queueName);
                receiveMessageRequest = new ReceiveMessageRequest();
                receiveMessageRequest.QueueUrl = queueUrl;
                receiveMessageRequest.MessageAttributeNames = new List<string> { "All" };
                receiveMessageRequest.MaxNumberOfMessages = 10;//max
                _queueUrl = queueUrl;


            }
            catch (Exception e)
            {
                throw new Exception("Fail to initialize Consumer: " + e.Message);

            }
            _workerThread = new Thread(new ThreadStart(this.start));
            _workerThread.Start();
        }
        private string getQueueUrl(string queueName)
        {
            GetQueueUrlResponse s = null;
            string result = string.Empty;
            try
            {
                GetQueueUrlRequest urlQueueRequest = new GetQueueUrlRequest
                {
                    QueueName = queueName
                };
                try
                {
                    s = amazonSQSClient.GetQueueUrlAsync(urlQueueRequest).Result;
                }
                catch (System.AggregateException)
                {
                    return createQueue(queueName);

                }
                if (s.QueueUrl.Any())
                {
                    return s.QueueUrl;
                }
                else
                {
                    return createQueue(queueName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                return createQueueResponse.QueueUrl;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void start()
        {
            isRunning = true;
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    ReceiveMessageResponse response = amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest).Result;
                    if (response.Messages.Any())
                    {

                        foreach (Amazon.SQS.Model.Message message in response.Messages)
                        {
                            if (receiver != null)
                            {
                                bool ack = true;
                                receiver.onMessage(message.Body, ack);
                                if (ack)
                                {
                                    var deleteMessageRequest = new DeleteMessageRequest();
                                    deleteMessageRequest.QueueUrl = _queueUrl;
                                    deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                                    DeleteMessageResponse  result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
                                }
                            }
                            
                        }
                    }


                }
            }
            catch (Exception e)
            {
                isRunning = false;
            }

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

    }
}

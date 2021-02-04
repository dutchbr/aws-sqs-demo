using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aws_sqs_demo.Controllers
{
    //* Controler de testes"

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QueueController : ControllerBase
    {



        [HttpGet]
        public string Echo()
        {
          //  AmazonSQSConfig sqsConfig = new AmazonSQSConfig();
           // sqsConfig.ServiceURL = "https://sqs.sa-east-1.amazonaws.com";
           // AmazonSQSClient sQSClient = new AmazonSQSClient(sqsConfig);
            
            return "oi";
        }

        [HttpGet]
        public string RegistrarCredenciais()
        {  //Apenas para desenvolver
            try
            {

                var options = new CredentialProfileOptions
                {
                    AccessKey = "",
                    SecretKey = ""
                };
                var profile = new Amazon.Runtime.CredentialManagement.CredentialProfile("root_profile", options);
                profile.Region = RegionEndpoint.SAEast1;
                var netSDKFile = new NetSDKCredentialsFile();
                netSDKFile.RegisterProfile(profile);
                return "Registrado";
            }
            catch (Exception e)
            {
                return "Erro" + e.Message;
            }
        }
        
        [HttpGet]
        public string ReceiveMessages()
        {
            StringBuilder sb = new StringBuilder();
            string queueUrl = "https://sqs.sa-east-1.amazonaws.com/416049594167/SAP_FATAL_ERROR";
            BasicAWSCredentials awsCreds = new BasicAWSCredentials("", "");
            AmazonSQSClient amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.SAEast1);

            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = queueUrl;
            //receiveMessageRequest.AttributeNames = new List<string> { "ApproximateReceiveCount" };
            receiveMessageRequest.MessageAttributeNames = new List<string> { "All" };
            receiveMessageRequest.MaxNumberOfMessages = 10;//max


            ReceiveMessageResponse response = amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest).Result;
            if (response.Messages.Any())
            {
                
                foreach (Amazon.SQS.Model.Message message in response.Messages)
                {
                    sb.AppendLine(message.Body);

                    //Se quiser deletar a mensagem  
                    //var deleteMessageRequest = new DeleteMessageRequest();
                    //deleteMessageRequest.QueueUrl = queueUrl;
                    //deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;

                   //DeleteMessageResponse  result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
                
                }
            }
            return sb.ToString();

        }





        [HttpGet]
        public string SendMessage()
        {
            string  queueUrl = "https://sqs.sa-east-1.amazonaws.com/416049594167/SAP_FATAL_ERROR";
            BasicAWSCredentials awsCreds = new BasicAWSCredentials("", "");
            AmazonSQSClient amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.SAEast1);
            SendMessageRequest sendRequest = new SendMessageRequest();
            sendRequest.QueueUrl = queueUrl;
            sendRequest.MessageBody = "{ 'message' : 'hello world2' }";
            var sendMessageResponse = amazonSQSClient.SendMessageAsync(sendRequest).Result;
            
            return "sent";
            
        }


        [HttpGet]
        public string Teste()
        {
            BasicAWSCredentials awsCreds = new BasicAWSCredentials("", "");
            AmazonSQSClient amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.SAEast1);
           
            var createQueueRequest = new CreateQueueRequest();
            createQueueRequest.QueueName = "MySQSQueue";
            var attrs = new Dictionary<string, string>();
            attrs.Add(QueueAttributeName.VisibilityTimeout, "10");
            attrs.Add(QueueAttributeName.DelaySeconds, "90");
            attrs.Add(QueueAttributeName.MessageRetentionPeriod, "");



            createQueueRequest.Attributes = attrs;
            var createQueueResponse = amazonSQSClient.CreateQueueAsync(createQueueRequest).Result;






            return "oi";
        }














    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRex.Metadata;
using Microsoft.Azure.AppService.ApiApps.Service;
using System.Threading.Tasks;
using Microsoft.Azure;
using TVH.ApiApps.ServiceBusClaimCheck.Models;
using System.IO;
using System.Globalization;
using TVH.ServiceBusClaimCheck;
using System.Web;
using TVH.ServiceBus;
using Newtonsoft.Json;

namespace TVH.ApiApps.ServiceBusClaimCheck.Controllers
{
    public class ServiceBusClaimCheckController : ApiController
    {
        private string _serviceBusConnString = CloudConfigurationManager.GetSetting("ServiceBusConnString");
        private string _blobStorageConnString = CloudConfigurationManager.GetSetting("BlobStorageConnString");
        private string _blobContainerName = CloudConfigurationManager.GetSetting("BlobContainerName");
        
        private const string _dateTimeFormat = "MM/dd/yyyy HH:mm:ss";

        [Trigger(TriggerType.Poll, typeof(QueueMessageCount))]
        [HttpGet, Route("api/servicebus/queue/messagecount")]
        [Metadata("Get Queue Message Count", "Returns number of messages in the queue", VisibilityType.Default)]
        public async Task<HttpResponseMessage> GetQueueMessageCount([FromUri]string queueName)
        {
            //Validate input
            var inputValidationResult = await ValidateInput(queueName, true);
            if (inputValidationResult.IsValid == false)
                return inputValidationResult.HttpResult;

            //Get and return message count
            var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, queueName);
            var messageCount = await serviceBusQueueClient.GetMessageCount();

            return Request.CreateResponse(HttpStatusCode.OK, new QueueMessageCount { messageCount = messageCount, queueName = queueName });
        }

        [HttpGet, Route("api/servicebus/queue/abandon")]
        [Metadata("Abandon Message from Queue", "Abandons a message in the queue", VisibilityType.Default)]
        public async Task<HttpResponseMessage> AbandonMessage([FromUri]string queueName, [FromUri]string lockToken)
        {
            //Validate input
            var inputValidationResult = await ValidateInput(queueName, lockToken);
            if (inputValidationResult.IsValid == false)
                return inputValidationResult.HttpResult;

            //Abandon message
            var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, queueName);
            await serviceBusQueueClient.AbandonMessage(lockToken);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet, Route("api/servicebus/queue/complete")]
        [Metadata("Complete Message from Queue", "Completes a message in the queue", VisibilityType.Default)]
        public async Task<HttpResponseMessage> CompleteMessage([FromUri]string queueName, [FromUri]string lockToken)
        {
            //Validate input
            var inputValidationResult = await ValidateInput(queueName, lockToken);
            if (inputValidationResult.IsValid == false)
                return inputValidationResult.HttpResult;

            //Complete message
            var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, queueName);
            await serviceBusQueueClient.CompleteMessage(lockToken);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost, Route("api/servicebus/queue/send")]
        [Metadata("Send Message to Queue", "Sends a message to the queue, using the claim check pattern", VisibilityType.Default)]
        public async Task<HttpResponseMessage> SendMessageToQueue([FromBody]SendMessageCommand sendMessageCommand)
        {
            string test = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
            HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
            
            //Validate input
            var inputValidationResult = await ValidateInput(sendMessageCommand);
            
            if (inputValidationResult.IsValid == false)
                return inputValidationResult.HttpResult;

            //Send message to queue
            var serviceBusClaimCheckClient = new ServiceBusClaimCheckClient(_blobStorageConnString, _blobContainerName, _serviceBusConnString, sendMessageCommand.QueueName);
            await serviceBusClaimCheckClient.SendMessageToQueue(new ServiceBusMessage
            {
                Content = sendMessageCommand.Content,
                ContentType = sendMessageCommand.ContentType,
                ScheduledEnqueueTimeUtc = string.IsNullOrEmpty(sendMessageCommand.ScheduledEnqueueTimeUtc) ? DateTime.MinValue : DateTime.Parse(sendMessageCommand.ScheduledEnqueueTimeUtc),
                Properties = sendMessageCommand.Properties == null ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(sendMessageCommand.Properties.ToString())
            });

            return Request.CreateResponse(HttpStatusCode.Accepted); 
        }

        [Trigger(TriggerType.Poll, typeof(ServiceBusMessage))]
        [HttpGet, Route("api/servicebus/queue/receive")]
        [Metadata("Receive Message from Queue", "Receives a message from the queue, using the claim check pattern", VisibilityType.Default)]
        public async Task<HttpResponseMessage> ReceiveMessageFromQueue([FromUri]string queueName)
        {
            //Validate input
            var inputValidationResult = await ValidateInput(queueName, false);
            if (inputValidationResult.IsValid == false)
                return (HttpResponseMessage)inputValidationResult.HttpResult;

            //Receive message from queue
            var serviceBusClaimCheckClient = new ServiceBusClaimCheckClient(_blobStorageConnString, _blobContainerName, _serviceBusConnString, queueName);
            var receivedServiceBusMessage = await serviceBusClaimCheckClient.ReceiveMessageFromQueue();

            //No message available, wait until next polling
            if(receivedServiceBusMessage == null)
                return Request.EventWaitPoll();
            
            return Request.EventTriggered(values: receivedServiceBusMessage, pollAgain: TimeSpan.FromSeconds(0));
        }

        private async Task<ValidationResult> ValidateInput(SendMessageCommand sendMessageCommand)
        {
            //Validate queue input
            var queueValidationResult = await ValidateInput(sendMessageCommand.QueueName, false);
            if (queueValidationResult.IsValid == false)
                return queueValidationResult;

            //Check if DateTime is in valid format
            DateTime enqueueDate;

            if (string.IsNullOrWhiteSpace(sendMessageCommand.ScheduledEnqueueTimeUtc) == false && DateTime.TryParseExact(sendMessageCommand.ScheduledEnqueueTimeUtc, _dateTimeFormat, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out enqueueDate) == false)
                return new ValidationResult { IsValid = false, HttpResult = Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("The scheduled enqueue time is not in the expected {0} format", _dateTimeFormat)) };

            //Return success
            return new ValidationResult { IsValid = true, HttpResult = Request.CreateResponse(HttpStatusCode.Accepted) };
        }

        private async Task<ValidationResult> ValidateInput(string queueName, string lockToken)
        {
            //Validate queue input
            var queueValidationResult = await ValidateInput(queueName, true);
            if (queueValidationResult.IsValid == false)
                return queueValidationResult;

            //Validate lock token is GUID
            Guid guid;
            if (Guid.TryParse(lockToken, out guid) == false)
                return new ValidationResult { IsValid = false, HttpResult = Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("The lock token is not a valid GUID")) };

            //Return success
            return new ValidationResult { IsValid = true, HttpResult = Request.CreateResponse(HttpStatusCode.Accepted) };
        }

        private async Task<ValidationResult> ValidateInput(string queueName, bool checkIfQueueExists)
        {
            //Validate input - Check if queueName is provided
            if (string.IsNullOrWhiteSpace(queueName))
                return new ValidationResult { IsValid = false, HttpResult = Request.CreateResponse(HttpStatusCode.BadRequest, "The parameter 'queueName' is mandatory") };

            //Validate input - Check if queue exists
            if (checkIfQueueExists)
            {
                var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, queueName);
                if (await serviceBusQueueClient.QueueExists() == false)
                    return new ValidationResult { IsValid = false, HttpResult = Request.CreateResponse(HttpStatusCode.BadRequest, String.Format("The queue '{0}' does not exist", queueName)) };
            }
            return new ValidationResult { IsValid = true, HttpResult = Request.CreateResponse(HttpStatusCode.Accepted) };
        }
    }
}

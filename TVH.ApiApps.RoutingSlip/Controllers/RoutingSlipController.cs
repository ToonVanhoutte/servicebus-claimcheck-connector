using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using TRex.Metadata;
using System.Threading.Tasks;
using TVH.ApiApps.RoutingSlip.Models;
using Microsoft.Azure;
using TVH.FileStorage;
using TVH.RoutingSlip;
using TVH.ServiceBusClaimCheck;
using TVH.ServiceBus;
using System.Collections;
using System.Text;
using Microsoft.Azure.AppService.ApiApps.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TVH.ApiApps.RoutingSlip.Controllers
{
    public class RoutingSlipController : ApiController
    {
        private const string routingSlipPropertyName = "fwk-routingslip";

        private string _serviceBusConnString = CloudConfigurationManager.GetSetting("ServiceBusConnString");
        private string _blobStorageConnString = CloudConfigurationManager.GetSetting("BlobStorageConnString");
        private string _blobContainerName = CloudConfigurationManager.GetSetting("BlobContainerName");
        private string _fileStorageConnectionString = CloudConfigurationManager.GetSetting("FileStorageConnString");
        private string _fileStorageShare = CloudConfigurationManager.GetSetting("FileStorageShare");

        [Trigger(TriggerType.Poll, typeof(AssignedRoutingSlipEvent))]
        [HttpGet, Route("api/routingslip/assign")]
        [Metadata("Assign Routing Slip", "Returns the routing slip content", VisibilityType.Default)]
        public async Task<HttpResponseMessage> AssignRoutingSlip([FromUri]AssignRoutingSlipCommand assignRoutingSlipCommand)
        {
            try
            {
                var routingSlip = await GetRoutingSlipXml(assignRoutingSlipCommand.RoutingSlipName);
                return Request.CreateResponse(HttpStatusCode.OK, new AssignedRoutingSlipEvent { RoutingSlip = routingSlip } );
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [Metadata("Publish Message", "Publishes message to the next step")]
        [HttpPost, Route("api/routingslip/publish")]
        public async Task<HttpResponseMessage> PublishMessage([FromBody]PublishMessageCommand publishMessageCommand)
        {
            try
            {
                var nextRoutingStep = new RoutingStep();
                var context = (publishMessageCommand.Context != null) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(publishMessageCommand.Context.ToString()) : null;
                var nextRoutingSlipXml = RoutingSlipClient.SetNextRouting(publishMessageCommand.RoutingSlip, context, out nextRoutingStep);

                if (nextRoutingStep != null)
                {
                    if (nextRoutingStep.StepName == "injectroutingslip")
                    {
                        var routingSlipToAppendXml = await GetRoutingSlipXml(nextRoutingStep.StepConfig.First(p => p.Name == "RoutingSlip").Value);
                        nextRoutingSlipXml = RoutingSlipClient.InjectRoutingSlip(nextRoutingSlipXml, routingSlipToAppendXml);
                        nextRoutingStep = RoutingSlipClient.GetCurrentRoutingStep(nextRoutingSlipXml);
                    }

                    //TODO Remove this!!
                    if (nextRoutingStep.StepName == "batch")
                    {
                        nextRoutingStep.StepName = RoutingSlipClient.GetCurrentStepProperties(nextRoutingSlipXml).First(p => p.Key == "BatchConfig").Value;
                    }

                    var serviceBusClaimCheckClient = new ServiceBusClaimCheckClient(_blobStorageConnString, _blobContainerName, _serviceBusConnString, nextRoutingStep.StepName);
                    await serviceBusClaimCheckClient.SendMessageToQueue(new ServiceBusMessage {
                        Content = publishMessageCommand.MessageBody,
                        Properties = new Dictionary<string, string> { { routingSlipPropertyName, nextRoutingSlipXml} }
                    });
                }

                return Request.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [Trigger(TriggerType.Poll, typeof(SubscribedMessageEvent))]
        [HttpGet, Route("api/routingslip/subscribe")]
        [Metadata("Subscribe Message", "Returns a message for the current service", VisibilityType.Default)]
        public async Task<HttpResponseMessage> SubscribeMessage([FromUri]SubscribeMessageCommand subscribeMessageCommand)
        {
            try
            {
                var queueName = subscribeMessageCommand.StepName;
                //Receive message from queue
                var serviceBusClaimCheckClient = new ServiceBusClaimCheckClient(_blobStorageConnString, _blobContainerName, _serviceBusConnString, queueName);
                var receivedServiceBusMessage = await serviceBusClaimCheckClient.ReceiveMessageFromQueue();

                //No message available, wait until next polling
                if (receivedServiceBusMessage == null)
                    return Request.EventWaitPoll();

                var routingSlipXml = receivedServiceBusMessage.Properties[routingSlipPropertyName];
                var routingSlipHeader = RoutingSlipClient.GetRoutingSlipHeader(routingSlipXml);

                return Request.CreateResponse(HttpStatusCode.OK, new SubscribedMessageEvent {
                    MessageBody = receivedServiceBusMessage.Content,
                    RoutingSlip = routingSlipXml,
                    LockToken = receivedServiceBusMessage.LockToken,
                    StepProperties = JObject.Parse(JsonConvert.SerializeObject(RoutingSlipClient.GetCurrentStepProperties(routingSlipXml))),
                    Context = JObject.Parse(JsonConvert.SerializeObject(RoutingSlipClient.GetContext(routingSlipXml))),
                    QueueName = queueName,
                    CorrelationId = routingSlipHeader.CorrelationId,
                    CurrentStepNumber = routingSlipHeader.CurrentStep,
                    Flow = routingSlipHeader.Flow
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [Trigger(TriggerType.Poll, typeof(SubscribedMessagesEvent))]
        [HttpGet, Route("api/routingslip/subscribe/batch")]
        [Metadata("Subscribe Message Batch", "Returns a batch of messages for the current service", VisibilityType.Default)]
        public async Task<HttpResponseMessage> SubscribeMessages([FromUri]SubscribeMessagesCommand subscribeMessagesCommand)
        {
            try
            {
                var queueName = subscribeMessagesCommand.StepName;
                //Receive message batch from queue
                var serviceBusClaimCheckClient = new ServiceBusClaimCheckClient(_blobStorageConnString, _blobContainerName, _serviceBusConnString, queueName);
                var receivedServiceBusMessages = await serviceBusClaimCheckClient.ReceiveMessagesFromQueue(subscribeMessagesCommand.Amount, TimeSpan.FromSeconds(subscribeMessagesCommand.TimeOut));

                //No message available, wait until next polling
                if (receivedServiceBusMessages == null || receivedServiceBusMessages.Count == 0)
                    return Request.EventWaitPoll();

                var routingSlipXml = receivedServiceBusMessages[0].Properties[routingSlipPropertyName];
                var routingSlipHeader = RoutingSlipClient.GetRoutingSlipHeader(routingSlipXml);

                return Request.CreateResponse(HttpStatusCode.OK, new SubscribedMessagesEvent
                {
                    MessageBatch = receivedServiceBusMessages.Select(x => x.Content).ToList(),
                    RoutingSlip = routingSlipXml,
                    LockTokens = receivedServiceBusMessages.Select(x => x.LockToken).ToList(),
                    StepProperties = JObject.Parse(JsonConvert.SerializeObject(RoutingSlipClient.GetCurrentStepProperties(routingSlipXml))),
                    Context = JObject.Parse(JsonConvert.SerializeObject(RoutingSlipClient.GetContext(routingSlipXml))),
                    QueueName = queueName,
                    CorrelationId = routingSlipHeader.CorrelationId,
                    CurrentStepNumber = routingSlipHeader.CurrentStep,
                    Flow = routingSlipHeader.Flow,
                    BatchSize = receivedServiceBusMessages.Count
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [HttpGet, Route("api/servicebus/queue/abandon")]
        [Metadata("Abandon Message from Queue", "Abandons a message in the queue", VisibilityType.Default)]
        public async Task<HttpResponseMessage> AbandonMessage([FromUri]AcknowledgeMessageCommand ackMessageCommand)
        {
            //Abandon message
            var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, ackMessageCommand.QueueName);
            await serviceBusQueueClient.AbandonMessage(ackMessageCommand.QueueName, ackMessageCommand.IgnoreLockLostException);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet, Route("api/servicebus/queue/complete")]
        [Metadata("Complete Message from Queue", "Completes a message in the queue", VisibilityType.Default)]
        public async Task<HttpResponseMessage> CompleteMessage([FromUri]AcknowledgeMessageCommand ackMessageCommand)
        {
            //Complete message
            var serviceBusQueueClient = new ServiceBusQueueClient(_serviceBusConnString, ackMessageCommand.QueueName);
            await serviceBusQueueClient.CompleteMessage(ackMessageCommand.LockToken, ackMessageCommand.IgnoreLockLostException);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<string> GetRoutingSlipXml(string routingSlipName)
        {
            if (routingSlipName.EndsWith(".xml") == false)
                routingSlipName += ".xml";

            var fileStorageClient = new FileStorageClient(_fileStorageConnectionString);
            var routingSlipBytes = await fileStorageClient.GetFileContent(_fileStorageShare, routingSlipName);
            return Encoding.UTF8.GetString(routingSlipBytes);
        }
    }
}

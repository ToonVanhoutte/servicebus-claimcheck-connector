using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TVH.ClaimCheck;
using TVH.ServiceBus;

namespace TVH.ServiceBusClaimCheck
{
    public class ServiceBusClaimCheckClient
    {
        private const string claimCheckPropertyName = "fwk-claimcheckuri";

        private IClaimCheckProvider _claimCheckProvider;
        private ServiceBusQueueClient _serviceBusQueueClient;
        public ServiceBusClaimCheckClient(string blobStorageConnString, string blobStorageContainerName, string serviceBusConnString, string queueName)
        {
            _claimCheckProvider = new BlobStorageClaimCheckProvider(blobStorageConnString, blobStorageContainerName);
            _serviceBusQueueClient = new ServiceBusQueueClient(serviceBusConnString, queueName);
        }

        public async Task SendMessageToQueue(ServiceBusMessage serviceBusMessage)
        {
            //Upload the message, get the message reference
            var messageReference = await _claimCheckProvider.StoreMessage(serviceBusMessage.Content);
            serviceBusMessage.Properties.Add(claimCheckPropertyName, messageReference);

            //Remove the message content
            serviceBusMessage.Content = Encoding.UTF8.GetBytes(string.Format("The message content is stored on blob storage on this location: {0}", messageReference));

            //Send the message to the queue
            await _serviceBusQueueClient.SendMessage(serviceBusMessage);
        }

        public async Task<ServiceBusMessage> ReceiveMessageFromQueue()
        {
            //Receive message from queue
            var serviceBusMessage = await _serviceBusQueueClient.ReceiveMessage();

            //No message available
            if (serviceBusMessage == null)
                return null;

            //Retrieve the message content from blob storage
            var messageContent = await _claimCheckProvider.RetrieveMessage(serviceBusMessage.Properties[claimCheckPropertyName].ToString());

            //Return message and poll immediately for new messages
            serviceBusMessage.Content = messageContent;
            return serviceBusMessage;
        }

        public async Task<List<ServiceBusMessage>> ReceiveMessagesFromQueue(int amount, TimeSpan timeOut)
        {
            //Receive messages from queue
            var serviceBusMessages = await _serviceBusQueueClient.ReceiveMessages(amount, timeOut);

            //No messages available
            if (serviceBusMessages == null || serviceBusMessages.Count == 0)
                return null;

            //Retrieve the messages content from blob storage
            foreach (var serviceBusMessage in serviceBusMessages)
            {
                var messageContent = await _claimCheckProvider.RetrieveMessage(serviceBusMessage.Properties[claimCheckPropertyName].ToString());
                serviceBusMessage.Content = messageContent;
            }

            return serviceBusMessages;
        }
    }
}

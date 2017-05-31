using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.ApiApps.ServiceBus
{
    public class ServiceBusQueueClient
    {
        private string _serviceBusConnectionString;
        public ServiceBusQueueClient(string connectionString)
        {
            _serviceBusConnectionString = connectionString;
        }

        public async Task<long> GetMessageCount(string queueName)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_serviceBusConnectionString);
            var queue = await namespaceManager.GetQueueAsync(queueName);
            return queue.MessageCount;
        }

        public async Task SendMessage(string queueName, ServiceBusMessage message)
        {
            var queueClient = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);
            
            var outputMessage = new BrokeredMessage(new MemoryStream(message.Content));
            outputMessage.ContentType = message.ContentType;

            if (message.Properties != null)
            {
                foreach (var prop in message.Properties)
                {
                    outputMessage.Properties.Add(prop.Key, prop.Value);
                }
            }

            if (message.ScheduledEnqueueTimeUtc != null)
                outputMessage.ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc;

            await queueClient.SendAsync(outputMessage);
        }

        public async Task<ServiceBusMessage> ReceiveMessage(string queueName)
        {
            var queueClient = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);

            var message = await queueClient.ReceiveAsync();

            if (message == null)
                return null;

            return new ServiceBusMessage
            {
                Content = message.GetBody<byte[]>(),
                ContentType = message.ContentType,
                Properties = message.Properties,
                LockToken = message.LockToken.ToString()
            };      
        }

        public async Task<List<ServiceBusMessage>> ReceiveMessages(string queueName, int amount, int timeOut)
        {
            var queueClient = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);
            var messages = await queueClient.ReceiveBatchAsync(amount, TimeSpan.FromSeconds(timeOut));

            var serviceBusMessages = new List<ServiceBusMessage>();
            foreach (var message in messages)
            {
                serviceBusMessages.Add(CreateServiceBusMessage(message));
            }
            return null;
        }

        public async Task<bool> QueueExists(string queueName)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_serviceBusConnectionString);
            return await namespaceManager.QueueExistsAsync(queueName);
        }

        public async Task AbandonMessage(string queueName, string lockToken)
        {
            var queueClient = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);
            await queueClient.AbandonAsync(Guid.Parse(lockToken));
        }
        public async Task CompleteMessage(string queueName, string lockToken)
        {
            var queueClient = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, queueName);
            await queueClient.CompleteAsync(Guid.Parse(lockToken));
        }

        public ServiceBusMessage CreateServiceBusMessage(BrokeredMessage message)
        {
            return new ServiceBusMessage
            {
                Content = message.GetBody<byte[]>(),
                ContentType = message.ContentType,
                Properties = message.Properties,
                LockToken = message.LockToken.ToString()
            };
        }
    }
}

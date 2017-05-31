using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.ServiceBus
{
    public class ServiceBusQueueClient
    {
        private NamespaceManager _namespaceManager;
        private QueueClient _queueClient;
        private string _queueName;
        public ServiceBusQueueClient(string connectionString, string queueName)
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _queueName = queueName;
            _queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);
        }

        public async Task<long> GetMessageCount()
        {
            var queue = await _namespaceManager.GetQueueAsync(_queueName);
            return queue.MessageCount;
        }

        public async Task SendMessage(ServiceBusMessage message)
        {
            await CreateQueueIfNotExists();
            
            var outputMessage = (message.Content != null) ? new BrokeredMessage(new MemoryStream(message.Content)) : new BrokeredMessage();
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

            await _queueClient.SendAsync(outputMessage);
        }

        public async Task<ServiceBusMessage> ReceiveMessage()
        {
            var message = await _queueClient.ReceiveAsync();

            if (message == null)
                return null;

            return CreateServiceBusMessage(message);
        }

        public async Task<List<ServiceBusMessage>> ReceiveMessages(int amount, TimeSpan timeOut)
        {
            await CreateQueueIfNotExists();
            await _queueClient.PeekAsync();
            var messages = await _queueClient.ReceiveBatchAsync(amount, timeOut);

            return messages.Select(m => CreateServiceBusMessage(m)).ToList();
        }

        public async Task<bool> QueueExists()
        {
            return await _namespaceManager.QueueExistsAsync(_queueName);
        }

        public async Task AbandonMessage(string lockToken)
        {
            await AbandonMessage(lockToken, false);
        }
        public async Task AbandonMessage(string lockToken, bool ignoreLockLostException)
        {
            try
            {
                await _queueClient.AbandonAsync(Guid.Parse(lockToken));
            }
            catch (MessageLockLostException ex)
            {
                if (ignoreLockLostException == false)
                    throw ex;
            }
        }

        public async Task CompleteMessage(string lockToken)
        {
            await CompleteMessage(lockToken, false);
        }
        public async Task CompleteMessage(string lockToken, bool ignoreLockLostException)
        {
            try
            {
                await _queueClient.CompleteAsync(Guid.Parse(lockToken));
            }
            catch (MessageLockLostException ex)
            {
                if (ignoreLockLostException == false)
                    throw ex;
            }
        }

        public ServiceBusMessage CreateServiceBusMessage(BrokeredMessage message)
        {
            var contentStream = message.GetBody<Stream>();
            byte[] content;
            using (var streamReader = new MemoryStream())
            {
                contentStream.CopyTo(streamReader);
                content = streamReader.ToArray();
            }

            return new ServiceBusMessage
            {
                Content = content,
                ContentType = message.ContentType,
                Properties = message.Properties.ToDictionary(p => p.Key, p => p.Value == null ? "" : p.Value.ToString()),
                LockToken = message.LockToken.ToString()
            };
        }


        private async Task CreateQueueIfNotExists()
        {
            bool queueExists = await _namespaceManager.QueueExistsAsync(_queueName);

            if (queueExists == false)
            {
                var queueDescription = new QueueDescription(_queueName);
                queueDescription.EnablePartitioning = false;
                await _namespaceManager.CreateQueueAsync(queueDescription);
            }
        }
    }
}

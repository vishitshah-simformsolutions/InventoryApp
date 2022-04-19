using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SIgnalR.Service
{
    public interface IServiceBusHelper
    {
        public Task PublishMessageToTopicAsync(string message);
    }

    public class ServiceBusHelper : IServiceBusHelper
    {
        private readonly ServiceBusClient _client;

        public ServiceBusHelper(ServiceBusClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Connect with ServiceBus client, create a sender topic path and send message to bus.
        /// </summary>
        /// <param name="servicesList"></param>
        /// <returns></returns>
        public async Task PublishMessageToTopicAsync(string bidResponseMessage)
        {
            ServiceBusSender sender = null;
            try
            {
                sender = _client.CreateSender("dev-sb-template");
                using var messageBatch = await sender.CreateMessageBatchAsync();

                // get the messages to be sent to the Service Bus topic
                var messages = CreateMessages(bidResponseMessage);

                if (messageBatch.TryAddMessage(messages.Peek()))
                {
                    messages.Dequeue();
                }
                // now, send the batch
                await sender.SendMessagesAsync(messageBatch);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sender != null)
                {
                    await sender.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Create messages to send and append to queue
        /// </summary>
        /// <param name="serviceList"></param>
        /// <returns></returns>
        private Queue<ServiceBusMessage> CreateMessages(string message)
        {
            var queue = new Queue<ServiceBusMessage>();
            var partitionKey = Guid.NewGuid().ToString();
            var msgId = Guid.NewGuid().ToString();

            var queueMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(message));
            queueMessage.MessageId = msgId;
            queueMessage.PartitionKey = partitionKey;

            queue.Enqueue(queueMessage);

            return queue.Count > 0 ? queue : null;
        }
    }
}

using System;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace hso.messages
{
    public static class MessageProcessing
    {
        [FunctionName("MessageProcessing")]
        public static void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            // Connection String & Queue to Read
            string connectionString = Environment.GetEnvironmentVariable("WEBSITE_QUEUELOCATION");
            string queueName = Environment.GetEnvironmentVariable("WEBSITE_QUEUENAME");

            // Parameters END

            // Setup Queue Reference
            QueueClient queue = new QueueClient(connectionString, queueName);

            // Get queue properties to find the number of messages in the queue, then setup integer for our do loop.
            QueueProperties properties = queue.GetProperties();
            int countmessages = properties.ApproximateMessagesCount;
            int i = 1;

            // Get messages
            do
            {

                foreach (QueueMessage message in queue.ReceiveMessages(maxMessages: 1).Value)
                {

                    // "Process" the message
                    log.LogInformation($"Message: {message.Body}");
                    // Let the service know we're finished with the message and
                    // it can be safely deleted.
                    queue.DeleteMessage(message.MessageId, message.PopReceipt);
                    i++;
                }

            } while (i <= countmessages);

        }

    }

}

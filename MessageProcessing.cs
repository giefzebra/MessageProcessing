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
        public static void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            // Connection String
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=sjrrothwellqueuestorlrn;AccountKey=3WgQK1u5DWCTLs6Di2z+/k2hWT6hJGpdBgWzH49PiwZJVNqn+P+yyE4anp3KFm44YOI+JQTaSz809CMciJfysA==;EndpointSuffix=core.windows.net";

            // Name of an existing queue we'll operate on
            string queueName = "orderprocessing";

            // Get a reference to a queue 
            QueueClient queue = new QueueClient(connectionString, queueName);

            // Get the next messages from the queue
            foreach (QueueMessage message in queue.ReceiveMessages(maxMessages: 32).Value)
            {
                // "Process" the message
                log.LogInformation($"Message: {message.Body}");
                // Let the service know we're finished with the message and
                // it can be safely deleted.
                queue.DeleteMessage(message.MessageId, message.PopReceipt);
            }
        }
    }
}

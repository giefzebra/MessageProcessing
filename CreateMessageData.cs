using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace hso.createmessages
{
    public static class CreateMessageData
    {
        [FunctionName("CreateMessageData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            // We'll need a connection string to your Azure Storage account.
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=sjrrothwellqueuestorlrn;AccountKey=3WgQK1u5DWCTLs6Di2z+/k2hWT6hJGpdBgWzH49PiwZJVNqn+P+yyE4anp3KFm44YOI+JQTaSz809CMciJfysA==;EndpointSuffix=core.windows.net";

            // Name of an existing queue we'll operate on
            string queueName = "orderprocessing";

            // Get a reference to a queue 
            QueueClient queue = new QueueClient(connectionString, queueName);

            // Create Random Order Number & Message Data

            Random rnd = new Random();
            int order = rnd.Next(1000000, 9000000);  // creates a number 
            int customer = rnd.Next(1000, 2000);

            string messageData = "{ 'OrderID': 'AO" + order + "', 'CustomerID': " + customer + "'}";

            // Send Message to Queue
            await InsertMessageAsync(queue, messageData);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        // Message Creation Method
        static async Task InsertMessageAsync(QueueClient theQueue, string newMessage)
        {


            if (null != await theQueue.CreateIfNotExistsAsync())
            {
                Console.WriteLine("The queue was created.");
            }

            await theQueue.SendMessageAsync(newMessage);
        }
    }
}

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

namespace hso.createmultiplemessages
{
    public static class CreateMultipleMessages
    {
        [FunctionName("CreateMultipleMessages")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Boilerplate Function request code
            // Function to take parameter of 'nummessages'
            string nummessages = req.Query["nummessages"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            nummessages = nummessages ?? data?.nummessages;

            // Convert nummessages to integer
            int nummessagestocreate;

            bool isParsable = Int32.TryParse(nummessages, out nummessagestocreate);
            if (isParsable)

            {
                log.LogInformation("Request for %1 messages received", nummessagestocreate);
                await createmessages(nummessagestocreate);
            }

            else
                log.LogInformation("Number not parsable");


            // Respond

            string responseMessage = string.IsNullOrEmpty(nummessages)
                ? "This HTTP triggered function executed successfully, however no messages were requested via the 'nummessages' parameter, please pass a numeric value via nummessages parameter to create sample data"
                : $"{nummessages} random messages created";

            return new OkObjectResult(responseMessage);

        }

        public static async Task<IActionResult> createmessages(int nummessages)
        {
            // Connection String & Queue to Read
            // Setup QueueClient to establish connection to queue

            string connectionString = Environment.GetEnvironmentVariable("WEBSITE_QUEUELOCATION");
            string queueName = Environment.GetEnvironmentVariable("WEBSITE_QUEUENAME");
            QueueClient queue = new QueueClient(connectionString, queueName);

            // Set loop variables ready for creation of messages
            int i = 0;
            int messagestocreate = nummessages;

            // Perform insertion of message data
            do

            {
                Random rnd = new Random();
                int order = rnd.Next(1000000, 9000000);  // creates a number 
                int customer = rnd.Next(1000, 2000);

                string messageData = "{ 'OrderID': 'AO" + order + "', 'CustomerID': " + customer + "'}";
                await queue.SendMessageAsync(messageData);

                i++;
            }

            while (i < messagestocreate);

            return new OkObjectResult("{messagestocreate} messages created");

        }
    }
}

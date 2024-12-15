using System;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using System.Net.Http;

namespace IIS_Listener_Function
{
    public class IISListenerFunction
    {
        private readonly ILogger _logger;

        private static EventHubProducerClient producerClient
            = new EventHubProducerClient(Environment.GetEnvironmentVariable("EventHubEndpoint"));

        private static HttpClient sharedClient = new() { };

        public IISListenerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IISListenerFunction>();
        }

        [Function("IISListenerFunction")]
        public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // We read: http://api.open-notify.org/iss-now.json

            var url = Environment.GetEnvironmentVariable("IISNowUrl") ;

            using HttpResponseMessage response = await sharedClient.GetAsync(url);

            var jsonResponse = await response.Content.ReadAsStringAsync();
      
            try
            {
                // Create a batch of events 
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                // Add json message
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(jsonResponse))))
                {
                    _logger.LogWarning("Batch did not accept JSON");
                }

                await producerClient.SendAsync(eventBatch);

                _logger.LogInformation($"JSON '{jsonResponse}' sent to endpoint");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while sending JSON to Custom App: '{ex.Message}'");
            }

            //if (myTimer.ScheduleStatus is not null)
            //{
            //    _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            //}
        }
    }
}

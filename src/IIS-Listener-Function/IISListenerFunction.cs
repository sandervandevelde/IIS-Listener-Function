using System;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using System.Net.Http;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;

namespace IIS_Listener_Function
{
    public class IISListenerFunction
    {
        private readonly ILogger _logger;

        private static EventHubProducerClient producerIisPositionClient
            = new EventHubProducerClient(Environment.GetEnvironmentVariable("EventHubIisPositionEndpoint"));

        private static HttpClient sharedClient = new() { };

        public IISListenerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IISListenerFunction>();
        }

        [Function("IIS-Position")]
        public async Task RunIisPosition([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger IISPositionUrl function executed at: {DateTime.Now}");

            // We read: http://api.open-notify.org/iss-now.json

            var iisNowUrl = Environment.GetEnvironmentVariable("IISNowUrl");

            using HttpResponseMessage iisNowResponse = await sharedClient.GetAsync(iisNowUrl);

            var jsonIisNowResponse = await iisNowResponse.Content.ReadAsStringAsync();

            _logger.LogInformation($"IisNow: {jsonIisNowResponse}");

            // We read: http://api.open-notify.org/astros.json and filter for IIS.

            var astrosUrl = Environment.GetEnvironmentVariable("AstrosUrl");

            using HttpResponseMessage astrosResponse = await sharedClient.GetAsync(astrosUrl);

            var jsonAstrosResponse = await astrosResponse.Content.ReadAsStringAsync();

            _logger.LogInformation($"Astros: {jsonAstrosResponse}");

            try
            {
                var iisNow = JsonSerializer.Deserialize<IisNow>(jsonIisNowResponse);

                var astros = JsonSerializer.Deserialize<Astros>(jsonAstrosResponse);

                var iisPeople = astros.people.Where(p => p.craft == "ISS").OrderBy(x=>x.name).ToArray();

                var iisAstros = new Astros
                {
                    message = $"{astros.message}-{iisNow.message}",
                    number = iisPeople.Length,
                    people = iisPeople,
                    timestamp = iisNow.timestamp,
                    iss_position = iisNow.iss_position
                };

                var iisAstrosJsonResponse = JsonSerializer.Serialize(iisAstros);

                // Create a batch of events 
                using EventDataBatch eventBatch = await producerIisPositionClient.CreateBatchAsync();

                // Add json message
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(iisAstrosJsonResponse))))
                {
                    _logger.LogWarning("Batch did not accept JSON");
                }

                await producerIisPositionClient.SendAsync(eventBatch);

                _logger.LogInformation($"JSON '{iisAstrosJsonResponse}' sent to endpoint");

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

        public class Astros
        {
            public string message { get; set; }
            public int number { get; set; }
            public People[] people { get; set; }
            public long timestamp { get; set; }
            public Iss_Position iss_position { get; set; }
        }

        public class People
        {
            public string craft { get; set; }
            public string name { get; set; }
        }

        public class IisNow
        {
            public string message { get; set; }
            public Iss_Position iss_position { get; set; }
            public long timestamp { get; set; }
        }

        public class Iss_Position
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
        }
    }
}

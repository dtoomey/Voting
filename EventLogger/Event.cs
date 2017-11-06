using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventLogger
{
    /// <summary>
    /// Event to be sent to Event Grid Topic.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// This will be used to update the Subject and Data properties.
        /// </summary>
        public LogEvent UpdateProperties
        {
            set
            {
                Subject = $"{value.AppName}-{value.LogEventType.ToString()}";
                Data = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier for the event.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the publisher defined path to the event subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets the registered event type for this event source.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// Gets the time the event is generated based on the provider's UTC time.
        /// </summary>
        public string EventTime { get; }

        /// <summary>
        /// Gets or sets the event data specific to the resource provider.
        /// </summary>
        public LogEvent Data { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Event()
        {
            Id = Guid.NewGuid().ToString();
            EventType = "logevent";
            EventTime = DateTime.UtcNow.ToString("o");
        }

        public static async Task Send(object events, string topicEndpoint, string key)
        {
            // Create a HTTP client which we will use to post to the Event Grid Topic
            var httpClient = new HttpClient();

            // Add key in the request headers
            httpClient.DefaultRequestHeaders.Add("aeg-sas-key", key);

            // Event grid expects event data as JSON
            var json = JsonConvert.SerializeObject(events);

            // Create request which will be sent to the topic
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send request
            Debug.WriteLine("Sending event to Event Grid...");
            var result = await httpClient.PostAsync(topicEndpoint, content);

            // Show result
            Debug.WriteLine($"Event sent with result: {result.ReasonPhrase}");
        }
    }
}

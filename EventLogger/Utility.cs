using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventLogger
{
    public class Utility
    {
        public async static Task SendLogEvent(string appName, LogEventType eventType, string message, string componentName, string methodName, string topicEndpoint, string topicKey)
        {
            List<Event> logEvents = new List<Event>();
            logEvents.Add(new Event()
            {
                UpdateProperties = new LogEvent(appName, eventType, message, componentName, methodName)
            });

            await Event.Send(logEvents, topicEndpoint, topicKey);
        }
    }
}

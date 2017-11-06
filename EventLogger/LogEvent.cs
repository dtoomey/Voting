namespace EventLogger
{
    public class LogEvent
    {
        public string AppName { get; set; }

        public string ComponentName { get; set; }

        public string MethodName { get; set; }

        public string Message { get; set; }

        public LogEventType LogEventType { get; set; }

        public LogEvent() { }

        public LogEvent(string appName, LogEventType logEventType, string message, string componentName, string methodName)
        {
            AppName = appName;
            LogEventType = logEventType;
            Message = message;
            ComponentName = componentName;
            MethodName = methodName;
        }

    }
}

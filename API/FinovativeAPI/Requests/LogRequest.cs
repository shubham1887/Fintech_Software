namespace Project.Requests
{
    public class LogRequest
    {
        public string Logger { get; internal set; }
        public string LogLevel { get; internal set; }
        public string Thread { get; internal set; }
        public string Message { get; internal set; }
        public string Exception { get; internal set; }
    }
}
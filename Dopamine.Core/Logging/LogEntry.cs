namespace Dopamine.Core.Logging
{
    public class LogEntry
    {
        public string TimeStamp { get; set; }
        public string Level { get; set; }
        public string CallerFilePath { get; set; }
        public string CallerMemberName { get; set; }
        public string Message { get; set; }
    }
}

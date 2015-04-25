using System;

namespace Common.History
{
    public class HistoryModel
    {
        public int Id { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string EventId { get; set; }
        public string WorkflowId { get; set; }
        public string HttpRequestType { get; set; }
        public string MethodCalledOnSender { get; set; }
        public string Message { get; set; }

        public HistoryModel()
        {
            TimeStamp = DateTime.Now;
        }
    }
}

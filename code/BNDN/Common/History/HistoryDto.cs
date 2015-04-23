using System;

namespace Common.History
{
    public class HistoryDto
    {
        public DateTime TimeStamp { get; private set; }
        public string EventId { get; set; }
        public string WorkflowId { get; set; }
        public string HttpRequestType { get; set; }
        public string MethodCalledOnSender { get; set; }
        public string Message { get; set; }

        public HistoryDto()
        {
            
        }
        public HistoryDto(HistoryModel model)
        {
            TimeStamp = model.TimeStamp;
            EventId = model.EventId;
            WorkflowId = model.WorkflowId;
            HttpRequestType = model.HttpRequestType;
            MethodCalledOnSender = model.MethodCalledOnSender;
            Message = model.Message;
        }
    }
}

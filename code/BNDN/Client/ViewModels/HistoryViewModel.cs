using System;
using Common.History;

namespace Client.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly HistoryDto _historyDto;
        public HistoryViewModel()
        {
            _historyDto = new HistoryDto();
        }
        public HistoryViewModel(HistoryDto historyDto)
        {
            _historyDto = historyDto;
        }

        #region DataBindings
        public string WorkflowId
        {
            get { return _historyDto.WorkflowId; }
            set
            {
                _historyDto.WorkflowId = value;
                NotifyPropertyChanged("WorkflowId");
            }
        }

        public string EventId
        {
            get { return _historyDto.EventId; }
            set
            {
                _historyDto.EventId = value;
                NotifyPropertyChanged("EventId");
            }
        }

        private string _title = "";
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Message
        {
            get { return _historyDto.Message; }
            set
            {
                _historyDto.Message = value;
                NotifyPropertyChanged("Message");
            }
        }

        public DateTime TimeStamp
        {
            get { return _historyDto.TimeStamp; }
        }
        
        #endregion

        #region Actions

        public override string ToString()
        {
            return _historyDto.EventId + ": " + _historyDto.Message;
        }

        #endregion

    }
}

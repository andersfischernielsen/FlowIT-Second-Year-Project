using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.History;

namespace Client.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private HistoryDto _historyDto;
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
        public string Message
        {
            get { return _historyDto.Message; }
            set
            {
                _historyDto.Message = value;
                NotifyPropertyChanged("Message");
            }
        }

        public DateTime TimeSpamp
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

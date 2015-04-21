using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.History;

namespace Client.ViewModels
{
    public class HistoryListViewModel : ViewModelBase
    {
        private Uri _serverAddress;
        
        public HistoryListViewModel()
        {
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>();

            var settings = Settings.LoadSettings();
            _serverAddress = new Uri(settings.ServerAddress);
            WorkflowId = "workflowID";

            HistoryViewModelList.Add(new HistoryViewModel(new HistoryDto(new HistoryModel{ EventId = "EventId1", Message = "Message1"})));
            HistoryViewModelList.Add(new HistoryViewModel(new HistoryDto(new HistoryModel { EventId = "EventId2", Message = "Message2" })));
        }

        public HistoryListViewModel(string workflowId)
        {
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>();
            WorkflowId = workflowId;

            var settings = Settings.LoadSettings();
            _serverAddress = new Uri(settings.ServerAddress);
        }

        #region DataBindings

        private string _workflowId;

        public string WorkflowId
        {
            get { return _workflowId; }
            set
            {
                _workflowId = value;
                NotifyPropertyChanged("WorkflowId");
            }
        }

        #endregion

        public ObservableCollection<HistoryViewModel> HistoryViewModelList { get; set; }


        #region Actions

        public Task GetHistory()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

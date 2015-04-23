using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Client.Connections;
using Common;
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
        }

        public HistoryListViewModel(string workflowId)
        {
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>();
            WorkflowId = workflowId;

            var settings = Settings.LoadSettings();
            _serverAddress = new Uri(settings.ServerAddress);

            GetHistory();
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

        /// <summary>
        /// Gets the history of the workflow and the events on it. 
        /// orders the list by timestamp
        /// </summary>
        /// <returns></returns>
        public async Task GetHistory()
        {
            HistoryViewModelList.Clear();

            // create a server connection
            IServerConnection serverConnection = new ServerConnection(new Uri(Settings.LoadSettings().ServerAddress));

            // get all addresses of events. This is neccesary since events might not be present if Adam removes events due to roles.
            var evenAddresses = (await serverConnection.GetEventsFromWorkflow(new WorkflowDto { Id = WorkflowId }))
                .AsParallel()
                .ToList();

            // add the history of the server
            ConcurrentBag<HistoryViewModel> history = new ConcurrentBag<HistoryViewModel>((await serverConnection.GetHistory(WorkflowId)).Select(dto => new HistoryViewModel(dto){Title = WorkflowId}));

            // add all the histories of the events.
            Parallel.ForEach(evenAddresses, async dto =>
            {
                IEventConnection eventConnection = new EventConnection(dto, WorkflowId);
                var list = (await eventConnection.GetHistory()).Select(historyDto => new HistoryViewModel(historyDto){Title = dto.Id});
                list.ToList().ForEach(model => history.Add(model));
            });

            // order them by timestamp
            var orderedHistory = history.ToList().OrderByDescending(model => model.TimeSpamp);

            // move the list into the observable collection.
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>(orderedHistory);
        }
        #endregion
    }
}

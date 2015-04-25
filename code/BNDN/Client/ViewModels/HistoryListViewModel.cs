using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Client.Connections;
using Common;

namespace Client.ViewModels
{
    public class HistoryListViewModel : ViewModelBase
    {
        private readonly Uri _serverAddress;

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
        public async void GetHistory()
        {
            HistoryViewModelList.Clear();
            NotifyPropertyChanged("");

            List<EventAddressDto> eventAddresses;
            ConcurrentBag<HistoryViewModel> history;

            // create a server connection
            using (IServerConnection serverConnection = new ServerConnection(_serverAddress))
            {

                // get all addresses of events. This is neccesary since events might not be present if Adam removes events due to roles.
                eventAddresses = (await serverConnection.GetEventsFromWorkflow(WorkflowId))
                    .AsParallel()
                    .ToList();

                // add the history of the server
                history = new ConcurrentBag<HistoryViewModel>((await serverConnection.GetHistory(WorkflowId)).Select(dto => new HistoryViewModel(dto) { Title = WorkflowId }));
            }

            var tasks = eventAddresses.Select(async dto =>
            {
                using (IEventConnection eventConnection = new EventConnection(dto.Uri))
                {
                    var list =
                        (await eventConnection.GetHistory(WorkflowId, dto.Id)).Select(
                            historyDto => new HistoryViewModel(historyDto) {Title = dto.Id});
                    list.ToList().ForEach(model => history.Add(model));
                }
            });

            await Task.WhenAll(tasks);

            // order them by timestamp
            // TODO: Currently, DateTimes are not parsed correctly.
            var orderedHistory = history.ToList().OrderByDescending(model => model.TimeStamp);

            // move the list into the observable collection.
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>(orderedHistory);
            NotifyPropertyChanged("");
        }
        #endregion
    }
}

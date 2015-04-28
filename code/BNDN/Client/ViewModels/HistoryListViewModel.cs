using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Client.Connections;
using Client.Exceptions;
using Common;
using Common.Exceptions;

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
        private string _status;

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

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

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

            IEnumerable<EventAddressDto> eventAddresses;
            ConcurrentBag<HistoryViewModel> history;

            try
            {
                // create a server connection
                using (IServerConnection serverConnection = new ServerConnection(_serverAddress))
                {
                    // get all addresses of events. This is neccesary since events might not be present if Adam removes events due to roles.
                    eventAddresses = await serverConnection.GetEventsFromWorkflow(WorkflowId);

                    // add the history of the server
                    history =
                        new ConcurrentBag<HistoryViewModel>(
                            (await serverConnection.GetHistory(WorkflowId)).Select(
                                dto => new HistoryViewModel(dto) {Title = WorkflowId}));
                }
            }
            catch (NotFoundException)
            {
                Status = "Workflow wasn't found on server. Please refresh the workflow and try again.";
                return;
            }
            catch (HostNotFoundException)
            {
                Status = "The server could not be found. Please try again later or contact your Flow administrator";
                return;
            }
            catch (Exception)
            {
                Status = "An unexpected error has occured. Please try again later.";
                return;
            }

            var tasks = eventAddresses.Select(async dto =>
            {
                using (IEventConnection eventConnection = new EventConnection(dto.Uri))
                {
                    var list =
                        (await eventConnection.GetHistory(WorkflowId, dto.Id)).Select(
                            historyDto => new HistoryViewModel(historyDto) {Title = dto.Id});
                    list.ToList().ForEach(history.Add);
                }
            });

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (NotFoundException)
            {
                Status = "An event wasn't found. Please refresh the workflow and try again.";
                return;
            }
            catch (HostNotFoundException)
            {
                Status = "An event-server could not be found. Please try again later or contact your Flow administrator";
                return;
            }
            catch (Exception)
            {
                Status = "An unexpected error has occured. Please try again later.";
                return;
            }

            // order them by timestamp
            var orderedHistory = history.ToList().OrderByDescending(model => model.TimeStamp);

            // move the list into the observable collection.
            HistoryViewModelList = new ObservableCollection<HistoryViewModel>(orderedHistory);
            NotifyPropertyChanged("");
        }
        #endregion
    }
}

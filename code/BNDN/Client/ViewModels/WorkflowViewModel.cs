using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Client.Connections;
using Client.Views;
using Common;

namespace Client.ViewModels
{
    public class WorkflowViewModel : ViewModelBase
    {
        private readonly WorkflowDto _workflowDto;
        private bool _resetEventRuns;
        private readonly WorkflowListViewModel _parent;

        public string WorkflowId { get { return _workflowDto.Id; } }

        public WorkflowViewModel(WorkflowListViewModel parent)
        {
            _parent = parent;
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = new WorkflowDto();
        }

        public WorkflowViewModel(WorkflowListViewModel parent, WorkflowDto workflowDto)
        {
            _parent = parent;
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = workflowDto;
        }

        #region Databindings

        public string Name
        {
            get { return _workflowDto.Name; }
            set
            {
                _workflowDto.Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public ObservableCollection<EventViewModel> EventList { get; set; }

        private EventViewModel _selectedEventViewModel;

        public EventViewModel SelectedEventViewModel
        {
            get { return _selectedEventViewModel; }
            set
            {
                _selectedEventViewModel = value;
                NotifyPropertyChanged("SelectedEventViewModel");
            }
        }

        public string Status
        {
            get { return _parent.Status; }
            set
            {
                _parent.Status = value;
            }
        }

        #endregion

        #region Actions

        public async void GetEvents()
        {
            SelectedEventViewModel = null;
            EventList.Clear();

            var settings = Settings.LoadSettings();
            var username = settings.Username;

            List<EventViewModel> events;
            using (IServerConnection connection = new ServerConnection(new Uri(Settings.LoadSettings().ServerAddress)))
            {
                events = (await connection.GetEventsFromWorkflow(WorkflowId))
                .AsParallel()
                .Where(e => e.Roles.Any(r => r == username)) //Only selects the events, the current user can execute
                .Select(eventAddressDto => new EventViewModel(eventAddressDto, this))
                .ToList();
            }
            
            EventList = new ObservableCollection<EventViewModel>(events);

            SelectedEventViewModel = EventList.Count >= 1 ? EventList[0] : null;
            
            NotifyPropertyChanged("");
        }

        /// <summary>
        /// Creates a new window with the log of the 
        /// </summary>
        public void GetHistory()
        {
            if (EventList != null && EventList.Count != 0)
            {
                var historyView = new HistoryView(new HistoryListViewModel(WorkflowId));
                historyView.Show();
            }
        }

        /// <summary>
        /// This method resets all the events on the workflow by deleting them and adding them again.
        /// This Method ONLY EXISTS FOR TESTING!
        /// This method is called when the button "Reset is called".
        /// </summary>
        public async void ResetWorkflow()
        {
            if (_resetEventRuns) return;
            _resetEventRuns = true;

            List<EventAddressDto> adminEventList;
            using (
                IServerConnection serverConnection = new ServerConnection(new Uri(Settings.LoadSettings().ServerAddress))
                )
            {
                adminEventList = (await serverConnection.GetEventsFromWorkflow(WorkflowId))
                .AsParallel()
                .ToList();
            }

            

            // Reset all the events.
            foreach (var eventViewModel in adminEventList)
            {
                using (IEventConnection connection = new EventConnection(eventViewModel.Uri))
                {
                    await connection.ResetEvent(WorkflowId, eventViewModel.Id);
                }
            }
            NotifyPropertyChanged("");
            GetEvents();
            _resetEventRuns = false;
        }
        #endregion

        /// <summary>
        /// This method is used by the list in the UI to represent each object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //.FormatString(this string myString) is an extension.
            return Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Client.Connections;
using Client.Exceptions;
using Client.Views;
using Common;
using Common.Exceptions;

namespace Client.ViewModels
{
    public class WorkflowViewModel : ViewModelBase
    {
        private readonly WorkflowDto _workflowDto;
        private bool _resetEventRuns;
        private readonly WorkflowListViewModel _parent;
        private readonly IList<string> _roles; 

        public string WorkflowId { get { return _workflowDto.Id; } }

        public WorkflowViewModel(WorkflowListViewModel parent)
        {
            _parent = parent;
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = new WorkflowDto();
            _roles = new List<string>();
        }

        public WorkflowViewModel(WorkflowListViewModel parent, WorkflowDto workflowDto, IList<string> roles)
        {
            _parent = parent;
            EventList = new ObservableCollection<EventViewModel>();
            _workflowDto = workflowDto;
            _roles = roles;
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

        public async void RefreshEvents()
        {
            var tasks = EventList.Select(async eventViewModel => await eventViewModel.GetState());
            await Task.WhenAll(tasks);
        }


        public async void GetEvents()
        {
            SelectedEventViewModel = null;
            EventList.Clear();

            var settings = Settings.LoadSettings();

            List<EventViewModel> events;
            using (IServerConnection connection = new ServerConnection(new Uri(settings.ServerAddress)))
            {
                events = (await connection.GetEventsFromWorkflow(WorkflowId))
                .AsParallel()
                .Where(e => e.Roles.Intersect(_roles).Any()) //Only selects the events, the current user can execute
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

            IEnumerable<EventAddressDto> adminEventList;
            try
            {
                using (IServerConnection serverConnection =
                    new ServerConnection(new Uri(Settings.LoadSettings().ServerAddress)))
                {
                    adminEventList = (await serverConnection.GetEventsFromWorkflow(WorkflowId));
                }
            }
            catch (NotFoundException)
            {
                Status = "The workflow wasn't found. Please refresh the list of workflows.";
                _resetEventRuns = false;
                return;
            }
            catch (HostNotFoundException)
            {
                Status = "The server is currently unavailable. Please try again later.";
                _resetEventRuns = false;
                return;
            }
            catch (Exception)
            {
                Status = "An unexpected error has occurred. Please refresh or try again later.";
                _resetEventRuns = false;
                return;
            }
            
            // Reset all the events.
            try
            {
                foreach (var eventViewModel in adminEventList)
                {
                    using (IEventConnection connection = new EventConnection(eventViewModel.Uri))
                    {
                        await connection.ResetEvent(WorkflowId, eventViewModel.Id);
                    }
                }
                NotifyPropertyChanged("");
                GetEvents();
            }
            catch (NotFoundException)
            {
                Status = "One of the events wasn't found. Please refresh the list of workflows.";
            }
            catch (HostNotFoundException)
            {
                Status = "An event-server is currently unavailable. Please try again later.";
            }
            catch (Exception)
            {
                Status = "An unexpected error has occurred. Please refresh or try again later.";
            }
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

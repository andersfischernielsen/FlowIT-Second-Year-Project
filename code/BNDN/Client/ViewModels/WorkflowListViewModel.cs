using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Client.Connections;
using Client.Exceptions;
using Common;

namespace Client.ViewModels
{
    public class WorkflowListViewModel : ViewModelBase
    {
        private readonly Uri _serverAddress;

        public WorkflowListViewModel()
        {
            WorkflowList = new ObservableCollection<WorkflowViewModel>();

            var settings = Settings.LoadSettings();
            _serverAddress = new Uri(settings.ServerAddress);

            GetWorkflows();
        }

        #region Databindings

        public ObservableCollection<WorkflowViewModel> WorkflowList { get; set; }

        private WorkflowViewModel _selecteWorkflowViewModel;
        private string _status;

        public WorkflowViewModel SelectedWorkflowViewModel
        {
            get { return _selecteWorkflowViewModel; }
            set
            {
                _selecteWorkflowViewModel = value;
                NotifyPropertyChanged("SelectedWorkflowViewModel");
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Is called to get all the workflows on the server. Events on the workflows are not retrieved.
        /// The method is called when the button "Refresh" is cliWcked.
        /// </summary>
        public async void GetWorkflows()
        {
            SelectedWorkflowViewModel = null;
            WorkflowList.Clear();

            IList<WorkflowDto> workflows;
            using (IServerConnection connection = new ServerConnection(_serverAddress))
            {
                try
                {
                    workflows = await connection.GetWorkflows();
                }
                catch (HostNotFoundException)
                {
                    Status = "The host of the server was not found. If the problem persists, contact you Flow administrator";
                    return;
                }
                catch (Exception e)
                {
                    _status = e.Message;
                    return;
                }
            }

            WorkflowList = new ObservableCollection<WorkflowViewModel>(workflows.Select(workflowDto => new WorkflowViewModel(this, workflowDto)));
            SelectedWorkflowViewModel = WorkflowList.Count >= 1 ? WorkflowList[0] : null;

            NotifyPropertyChanged("");
        }

        /// <summary>
        /// This method is called when the selection on the workflowList is changed.
        /// It gets all the event and in the end their states on the given workflow.
        /// </summary>
        public void GetEventsOnWorkflow()
        {
            if (SelectedWorkflowViewModel != null) SelectedWorkflowViewModel.GetEvents();
        }
        #endregion
    }
}

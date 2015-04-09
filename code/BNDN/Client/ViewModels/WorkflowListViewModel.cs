using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Client.ViewModels
{
    public class WorkflowListViewModel : ViewModelBase
    {
        private readonly Uri _serverAddress;

        public WorkflowListViewModel()
        {
            WorkflowList = new ObservableCollection<WorkflowViewModel>();

            if (File.Exists("settings.json"))
            {
                var settingsjson = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<Settings>(settingsjson);

                _serverAddress = new Uri(settings.ServerAddress ?? "http://flowit.azurewebsites.net/");
            }
            else
            {
                _serverAddress = new Uri("http://flowit.azurewebsites.net/");
            }

            GetWorkflows();
        }

        #region Databindings

        public ObservableCollection<WorkflowViewModel> WorkflowList { get; set; }

        private WorkflowViewModel _selecteWorkflowViewModel;

        public WorkflowViewModel SelectedWorkflowViewModel
        {
            get { return _selecteWorkflowViewModel; }
            set
            {
                _selecteWorkflowViewModel = value;
                NotifyPropertyChanged("SelectedWorkflowViewModel");
            }
        }

        #endregion

        #region Actions

        public async void GetWorkflows()
        {
            SelectedWorkflowViewModel = null;
            WorkflowList.Clear();

            //TODO: Implement exception handling.
            var connection = new ServerConnection(_serverAddress);

            var workflows = await connection.GetWorkflows();

            WorkflowList = new ObservableCollection<WorkflowViewModel>(workflows.Select(workflowDto => new WorkflowViewModel(workflowDto)));
            SelectedWorkflowViewModel = WorkflowList.Count >= 1 ? WorkflowList[0] : null;

            NotifyPropertyChanged("");
        }

        public void GetEventsOnWorkflow()
        {
            if (SelectedWorkflowViewModel != null) SelectedWorkflowViewModel.GetEvents();
        }
        #endregion
    }
}

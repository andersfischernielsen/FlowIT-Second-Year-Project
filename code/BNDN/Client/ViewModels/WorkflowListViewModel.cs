using System;
using System.Collections.ObjectModel;
using System.Linq;
using Common;

namespace Client.ViewModels
{
    public class WorkflowListViewModel : ViewModelBase
    {
        public WorkflowListViewModel()
        {
            WorkflowList = new ObservableCollection<WorkflowViewModel>();
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
            var connection = new ServerConnection(new Uri("http://localhost:13768/")); // todo get the real server address here

            var test = await connection.GetWorkflows();
            WorkflowList = new ObservableCollection<WorkflowViewModel>(test.Select(workflowDto => new WorkflowViewModel(workflowDto)));
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

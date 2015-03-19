using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class WorkflowListViewModel : ViewModelBase
    {
        public WorkflowListViewModel()
        {
            WorkflowList = new ObservableCollection<WorkflowViewModel>();
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

        public void GetWorkflows()
        {
            Task.Run(async () =>
            {
                WorkflowList.Clear();
                var connection = ServerConnection.GetStorage(new Uri("servers")); // todo get the real server address here
                WorkflowList = new ObservableCollection<WorkflowViewModel>((await connection.GetWorkflows()).Select(workflowDto => new WorkflowViewModel(workflowDto)));
                if (WorkflowList.Count >= 1)
                {
                    SelectedWorkflowViewModel = WorkflowList[0];
                }
                NotifyPropertyChanged("");
            });
        }

        public void GetEventsOnWorkflow()
        {
            SelectedWorkflowViewModel.GetEvents();
        }
        #endregion
    }
}

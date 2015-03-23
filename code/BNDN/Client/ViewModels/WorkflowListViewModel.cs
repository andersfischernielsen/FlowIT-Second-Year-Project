using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.ViewModels
{
    public class WorkflowListViewModel : ViewModelBase
    {
        public WorkflowListViewModel()
        {
            WorkflowList = new ObservableCollection<WorkflowViewModel>();
            WorkflowList.Add(new WorkflowViewModel(new WorkflowDto()){Name = "Workflow1", EventList = new ObservableCollection<EventViewModel>()
            {
                new EventViewModel(){Id = "ID11", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID12", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID13", Uri = new Uri("https://www.google.dk/")},
            }});
            WorkflowList.Add(new WorkflowViewModel(new WorkflowDto())
            {
                Name = "Workflow2",
                EventList = new ObservableCollection<EventViewModel>()
            {
                new EventViewModel(){Id = "ID21", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID22", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID23", Uri = new Uri("https://www.google.dk/")},
            }
            }); WorkflowList.Add(new WorkflowViewModel(new WorkflowDto())
            {
                Name = "Workflow3",
                EventList = new ObservableCollection<EventViewModel>()
            {
                new EventViewModel(){Id = "ID31", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID32", Uri = new Uri("https://www.google.dk/")},
                new EventViewModel(){Id = "ID33", Uri = new Uri("https://www.google.dk/")},
            }
            });
            SelectedWorkflowViewModel = WorkflowList[0];
            //GetWorkflows();
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
                #if DEBUG
                var connection = ServerConnection.GetStorage(new Uri("http://localhost:13768/")); // todo get the real server address here
                #else
                var connection = ServerConnection.GetStorage(new Uri("servers"));
                #endif

                WorkflowList = new ObservableCollection<WorkflowViewModel>((await connection.GetWorkflows()).Select(workflowDto => new WorkflowViewModel(workflowDto)));
                if (WorkflowList.Count >= 1)
                {
                    SelectedWorkflowViewModel = WorkflowList[0];
                }
                else
                {
                    SelectedWorkflowViewModel = null;
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

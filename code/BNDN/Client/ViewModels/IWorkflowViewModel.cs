using System.Threading.Tasks;

namespace Client.ViewModels
{
    public interface IWorkflowViewModel
    {
        string Status { get; set; }
        string WorkflowId { get; }
        Task DisableExecuteButtons();
        void RefreshEvents();
    }
}

using System.Threading.Tasks;
using Common;

namespace Client
{
    /// <summary>
    /// Connection to an event
    /// </summary>
    public interface IEventConnection
    {
        /// <summary>
        /// Get the state of a task
        /// </summary>
        /// <returns></returns>
        Task<EventStateDto> GetState();
        /// <summary>
        /// Execute a task
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        Task Execute(bool b, string workflowId);
    }
}

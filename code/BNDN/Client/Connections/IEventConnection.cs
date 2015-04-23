using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.History;

namespace Client.Connections
{
    /// <summary>
    /// Connection to an event
    /// </summary>
    public interface IEventConnection : IDisposable
    {
        /// <summary>
        /// Get the state of a task
        /// </summary>
        /// <returns></returns>
        Task<EventStateDto> GetState();

        /// <summary>
        /// Returns the history of the event.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<HistoryDto>> GetHistory();

        /// <summary>
        /// Delete an event. Only to be used for testing!
        /// TODO: REMOVE THIS METHOD WHEN DONE WITH TESTING.
        /// </summary>
        /// <returns></returns>
        Task ResetEvent();

        /// <summary>
        /// Execute a task
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        Task Execute(bool b);
    }
}

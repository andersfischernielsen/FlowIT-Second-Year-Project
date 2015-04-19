using System.Data.Entity;
using System.Threading.Tasks;
using Event.Interfaces;

namespace Event.Storage
{
    public class EventStorageForReset : IEventStorageForReset
    {
        private readonly IEventContext _context;

        public EventStorageForReset(IEventContext context)
        {
            _context = context;
        }

        public async Task ClearLock(string workflowId, string eventId)
        {
            // Clear any LockDto-element (should only exist a single)
            var @event = await _context.Events.SingleAsync(e => e.WorkflowId == workflowId && e.Id == eventId);

            @event.LockOwner = null;
            await _context.SaveChangesAsync();
        }

        public async Task ResetToInitialState(string workflowId, string eventId)
        {
            var @event = await _context.Events.SingleAsync(e => e.WorkflowId == workflowId && e.Id == eventId);

            @event.Executed = @event.InitialExecuted;
            @event.Included = @event.InitialIncluded;
            @event.Pending = @event.InitialPending;

            // Save changes
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
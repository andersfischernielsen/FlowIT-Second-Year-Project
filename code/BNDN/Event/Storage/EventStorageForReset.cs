using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Event.Interfaces;
using Event.Models;

namespace Event.Storage
{
    public class EventStorageForReset : IEventStorageForReset
    {
        private string _eventId;
        private IEventContext _context;

        public EventStorageForReset(string eventId, IEventContext context)
        {
            _eventId = eventId;
            _context = context;
        }

        public void ClearLock()
        {
            // Clear any LockDto-element (should only exist a single)
            foreach (var lockDto in _context.LockDto.Where(model => model.Id == _eventId))
            {
                _context.LockDto.Remove(lockDto);
            }
            _context.SaveChanges();
        }

        public void ResetToInitialState()
        {
            // Retrieve initial state
            var initialState = _context.InitialEventState.Single(x => x.EventId == _eventId);
            
            // Extract current state
            var currentState = _context.EventState.Single(x => x.Id == _eventId);

            var replacingState = new EventStateModel()
            {
                EventIdentificationModel = currentState.EventIdentificationModel,
                Id = currentState.Id,
                Executed = initialState.Executed,
                Included = initialState.Included,
                Pending = initialState.Pending
            };

            // Remove old
            _context.EventState.Remove(currentState);

            // Replace with new
            _context.EventState.Add(replacingState);

            // Save changes
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
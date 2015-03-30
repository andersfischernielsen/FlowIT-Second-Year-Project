using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common;

namespace Event.Models
{
    public class LockLogic
    {
        private readonly EventLogic _logic;
        private IEnumerable<KeyValuePair<Uri, List<NotifyDto>>> _list;
        private Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> _taskList;
        private HashSet<Uri> _locked; 
        public LockLogic()
        {
            _logic = EventLogic.GetState();
            _taskList = _logic.GetNotifyDtos(); // ER DET HER DEN RIGTIGE LISTE?
            _locked = new HashSet<Uri>();
        }

        public async Task<bool> LockAll()
        {
            if (_list == null)
            {
                _list = await _taskList;
            }
            try
            {
                LockDto lockDto = new LockDto() {LockOwner = _logic.EventId};
                _logic.LockDto = lockDto;

                foreach (var pair in _list)
                {
                    new EventCommunicator(pair.Key).Lock(lockDto).Wait();
                    _locked.Add(pair.Key);
                }
                //TODO: Brug Parrallel i stedet for
                //Parallel.ForEach(_list, async pair =>
                //{
                //    await new EventCommunicator(pair.Key).Lock(lockDto);
                //    _locked.TryAdd(pair.Key, 0);
                //});

                return true;
            }
            catch (Exception)
            {
                UnlockSome();
                return false;
            }
        }

        private void UnlockSome()
        {
            EventAddressDto eventAddress = new EventAddressDto() {Id = _logic.EventId, Uri = _logic.OwnUri};
            var parallelTasks = Parallel.ForEach(_locked, async pair =>
            {
                try
                {
                    new EventCommunicator(pair).Unlock(eventAddress).Wait();
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                }
                
            });
            while (!parallelTasks.IsCompleted)
            {
            }
            _locked = null;
        }

        public async Task<bool> UnlockAll()
        {

            if (_list == null)
            {
                _list = await _taskList;
            }
            bool b = true;

            var eventAddress = new EventAddressDto() { Id = _logic.EventId, Uri = _logic.OwnUri };

            var parallelTasks = Parallel.ForEach(_list, async pair =>
            {
                try
                {
                    new EventCommunicator(pair.Key).Unlock(eventAddress).Wait();
                }
                catch (Exception)
                {
                     // TODO: Find out what to do if you cant even unlock. Even.
                    b = false;
                }
                
            });
            while (!parallelTasks.IsCompleted)
            {
            }

            _logic.LockDto = null;
            return b;
        }
    }
}
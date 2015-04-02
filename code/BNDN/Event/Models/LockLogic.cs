using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Models
{
    public class LockLogic
    {
        private readonly EventLogic _logic;
        private readonly IEnumerable<Uri> _list;
        private HashSet<Uri> _locked; 
        public LockLogic()
        {
            _logic = new EventLogic();
            _list = _logic.GetNotifyDtos().Result; // ER DET HER DEN RIGTIGE LISTE?
            _locked = new HashSet<Uri>();
        }

        public async Task<bool> LockAll()
        {
            if (_list == null)
            {
                throw new InvalidOperationException("_list is not instantiated");
            }
            try
            {
                LockDto lockDto = new LockDto() {LockOwner = _logic.EventId};
                _logic.LockDto = lockDto;

                foreach (var uri in _list)
                {
                    await new EventCommunicator(uri).Lock(lockDto);
                    _locked.Add(uri);
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

            var parallelTasks = Parallel.ForEach(_locked, pair =>
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
            _logic.LockDto = null;
        }

        public async Task<bool> UnlockAll()
        {

            if (_list == null)
            {
                throw new InvalidOperationException("_list must not be null");
            }
            bool b = true;

            var eventAddress = new EventAddressDto() { Id = _logic.EventId, Uri = _logic.OwnUri };

            var parallelTasks = Parallel.ForEach(_list, async uri =>
            {
                try
                {
                    await new EventCommunicator(uri).Unlock(eventAddress);
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
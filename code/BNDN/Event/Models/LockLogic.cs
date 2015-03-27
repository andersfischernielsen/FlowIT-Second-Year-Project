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
        private ConcurrentDictionary<Uri, byte> _locked; 
        public LockLogic()
        {
            _logic = EventLogic.GetState();
            _taskList = _logic.GetNotifyDtos(); // ER DET HER DEN RIGTIGE LISTE?
            _locked = new ConcurrentDictionary<Uri, byte>();
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
                
                //Cancelation stuff
                CancellationTokenSource cts = new CancellationTokenSource();
                ParallelOptions po = new ParallelOptions();
                po.CancellationToken = cts.Token;
                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                
                //TODO: Få cancelationtoken helt på plads
                Parallel.ForEach(_list, async pair =>
                {
                    await new EventCommunicator(pair.Key).Lock(lockDto);
                    _locked.TryAdd(pair.Key, 0);
                });

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
            Parallel.ForEach(_locked, async pair =>
            {
                await new EventCommunicator(pair.Key).Unlock(eventAddress);
            });
            _locked = null;
        }

        public async Task<bool> UnlockAll()
        {

            if (_list == null)
            {
                _list = await _taskList;
            }

            var eventAddress = new EventAddressDto() { Id = _logic.EventId, Uri = _logic.OwnUri };
            try
            {
                Parallel.ForEach(_list, async pair =>
                {
                    await new EventCommunicator(pair.Key).Unlock(eventAddress);
                });
            }
            catch (Exception)
            {
                return false;
            }

            _logic.LockDto = null;
            return true;
        }
    }
}
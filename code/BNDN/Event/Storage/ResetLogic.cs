using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Event.Interfaces;

namespace Event.Storage
{
    public class ResetLogic : IResetLogic 
    {
        private IEventStorageForReset Storage { get; set; }

        public ResetLogic(string eventId)
        {
            Storage = new EventStorageForReset(eventId,new EventContext());
        }
        
        public void UnlockEvent()
        {
            Storage.ClearLock();
        }

        public void ResetToInitialValues()
        {
            Storage.ResetToInitialState();
        }

        public void Dispose()
        {
            Storage.Dispose();
        }
    }
}
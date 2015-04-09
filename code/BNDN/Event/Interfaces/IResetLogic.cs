using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    interface IResetLogic : IDisposable
    {
        /// <summary>
        /// Unlock will (brute) unlock the Event
        /// </summary>
        void UnlockEvent();

        /// <summary>
        /// ResetToInitialValues resets the Event to its initial bool-values for 
        /// Included, Pending and Executed
        /// </summary>
        void ResetToInitialValues();
    }
}

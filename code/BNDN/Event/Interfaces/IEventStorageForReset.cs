using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    interface IEventStorageForReset : IDisposable
    {
        void ClearLock();
        void ResetToInitialState();
    }
}

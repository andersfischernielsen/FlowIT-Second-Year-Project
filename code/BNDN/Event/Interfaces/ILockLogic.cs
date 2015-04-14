using System;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    public interface ILockLogic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A task containing a bool value indicating whether the operation of locking all related, dependent
        /// events was successfull or not </returns>
        Task<bool> LockAll();

        Task<bool> UnlockAll();
    }
}
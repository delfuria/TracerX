﻿using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace TracerX
{
    /// <summary>
    /// Handy pattern for creating, acquiring, releasing, and disposing a named mutex.
    /// The constructor creates and acquires.  Dispose() releases and disposes.
    /// </summary>
    public class NamedMutexWait : IDisposable
    {
        /// <summary>
        /// Creates and acquires a Mutex with the specified name 
        /// (should be prefixed with "Global\" or "Local\").
        /// If timeoutMs is not greater than 0, the wait is infinite.
        /// </summary>
        public NamedMutexWait(string name, int timeoutMs, bool throwOnTimeout)
        {
            // Remove due to Compatibility Issue
            //MutexAccessRule rule = new MutexAccessRule(
            //    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            //    MutexRights.Synchronize | MutexRights.Modify,
            //    AccessControlType.Allow);
            //MutexSecurity mSec = new MutexSecurity();
            //mSec.AddAccessRule(rule);
            //mutex = new Mutex(false, name, out DidCreate);

            mutex = new Mutex(false, name, out DidCreate);

            try
            {
                if (timeoutMs <= 0)
                {
                    DidAcquire = mutex.WaitOne(Timeout.Infinite, false);
                }
                else
                {
                    DidAcquire = mutex.WaitOne(timeoutMs, false);
                }

                if (DidAcquire == false && throwOnTimeout)
                {
                    throw new TimeoutException("Timeout waiting for exclusive access on NamedMutex");
                }
            }
            catch (AbandonedMutexException)
            {
                DidAcquire = true;
            }
        }

        /// <summary>
        /// Name of global mutex that serializes access to files in the global TracerX data folder.
        /// </summary>
        public const string DataDirMUtexName = @"Global\TracerXDataDir";

        public readonly bool DidCreate;
        public readonly bool DidAcquire;
        private Mutex mutex;

        /// <summary>
        /// Releases and disposes the mutex.
        /// </summary>
        public void Dispose()
        {
            if (mutex != null)
            {
                if (DidAcquire)
                {
                    mutex.ReleaseMutex();
                }

                (mutex as IDisposable).Dispose();
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ToDo.API.Client
{
    partial class Client : ClientBase
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Instantiate a SafeHandle instance.
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();

                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.

            disposed = true;

            // Call base class implementation.
            base.Dispose(disposing);
        }
    }
}

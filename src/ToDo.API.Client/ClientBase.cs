using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace ToDo.API.Client
{
    public class ClientBase : IDisposable
    {
        private protected readonly HttpClient HttpClient;
        private protected readonly ClientOptions ClientOptions;
        private protected readonly string BaseUrl;

        public ClientBase(ClientOptions clientOptions)
        {
            HttpClient = new HttpClient();
            ClientOptions = clientOptions;
            BaseUrl = clientOptions.ApiUrl;
        }

        #region Dispose Code
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Instantiate a SafeHandle instance.
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();

                // Free any other managed objects here.
                HttpClient.Dispose();
            }
            disposed = true;
        }
        #endregion

        protected async Task<HttpClient> CreateHttpClientAsync(System.Threading.CancellationToken cancellationToken)
        {
            return HttpClient;
        }

        protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(System.Threading.CancellationToken cancellationToken)
        {
            return new HttpRequestMessage();
        }
    }
}

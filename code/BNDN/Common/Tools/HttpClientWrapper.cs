﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common.Tools
{
    class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpRequestHeaders DefaultRequestHeaders
        {
            get { return _httpClient.DefaultRequestHeaders; }
        }

        public Uri BaseAddress
        {
            get { return _httpClient.BaseAddress; }
            set { _httpClient.BaseAddress = value; }
        }

        public TimeSpan Timeout
        {
            get { return _httpClient.Timeout; }
            set { _httpClient.Timeout = value; }
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string uri, T obj)
        {
            return await _httpClient.PostAsJsonAsync(uri, obj);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string uri, T obj)
        {
            return await _httpClient.PutAsJsonAsync(uri, obj);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {
            return await _httpClient.DeleteAsync(uri);
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            return await _httpClient.GetAsync(uri);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

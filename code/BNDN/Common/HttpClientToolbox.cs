using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// <para>The class contains a static Dictionary which all instances of the class will be added to. That way one can access different httpClients without having to create them all over again (for example if login is required each time).</para>
    /// <para>Instantiate the class with a string / uri which represents the rest api's base address for example http://driveit.azurewebsites.net/api/. </para>
    /// <para> Then when you make the CRUD calls, add the object and the rest of the uml eg "cars". If you dont know the object type, just use object. </para>
    /// </summary>
    public class HttpClientToolbox
    {
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Get/set the authetication header to the given value. Used for API's which needs authentication.
        /// </summary>
        public AuthenticationHeaderValue AuthenticationHeader
        {
            get { return HttpClient.DefaultRequestHeaders.Authorization; }
            set { HttpClient.DefaultRequestHeaders.Authorization = value; }
        }

        /// <summary>
        /// Instantiate a HttpClientToolbox with a given URL (as a string).
        /// </summary>
        /// <param name="uri">Uri (as a string) to use.</param>
        /// <param name="authenticationHeader">Optional authentocationheader.</param>
        public HttpClientToolbox(string uri, AuthenticationHeaderValue authenticationHeader = null)
        {
            HttpClient = new HttpClient { BaseAddress = new Uri(uri) };
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationHeader != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }
        }

        /// <summary>
        /// Instantiate a HttpClientToolbox with a given URI.
        /// </summary>
        /// <param name="uri">Uri to use.</param>
        /// <param name="authenticationHeader">Optional authentocationheader.</param>
        public HttpClientToolbox(Uri uri, AuthenticationHeaderValue authenticationHeader = null)
        {
            HttpClient = new HttpClient { BaseAddress = uri };
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationHeader != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }
        }

        /// <summary>
        /// Resets the HttpClient with the base address.
        /// </summary>
        /// <param name="uri"></param>
        public void SetBaseAddress(string uri)
        {
            HttpClient = new HttpClient { BaseAddress = new Uri(uri) };
        }

        /// <summary>
        /// Resets the HttpClient with the base address.
        /// </summary>
        /// <param name="uri"></param>
        public void SetBaseAddress(Uri uri)
        {
            HttpClient = new HttpClient { BaseAddress = uri };
        }

        /// <summary>
        /// Creates a POST http request with the type T to the address (baseaddress + URI). 
        /// T and the URI string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object at the address (baseaddress + URI)</typeparam>
        /// <param name="uri">A URI to the API (baseaddress + uri) where objects of type T are stored.</param>
        /// <param name="toCreate"> The type of object to send to the API.</param>
        /// <returns>An object that was created at the API.</returns>
        public virtual async Task Create<T>(string uri, T toCreate)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(uri, objectToCreate);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public virtual async Task<TResult> Create<TArgument, TResult>(string uri, TArgument objectToPost)
        {
            var response = await HttpClient.PostAsJsonAsync(uri, objectToPost);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<TResult>();
            return result;
        }

        /// <summary>
        /// Reads all the objects of type T at the API at (base address + URI). 
        /// T and the URI string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object at the URL (base address + URI).</typeparam>
        /// <param name="uri">The URI of the API where objects of type T are stored.</param>
        /// <returns>All objects of type T at the API.</returns>
        public virtual async Task<IList<T>> ReadList<T>(string uri)
        {
            T[] objects;
            try
            {
                var response = await HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                objects = await response.Content.ReadAsAsync<T[]>();
                return objects.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Reads an object of type T at the API at (base address + URI). 
        /// T and the URI string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object at the URL (base address + URI).</typeparam>
        /// <param name="uri">The URI of the API where an object of type T is stored.</param>
        /// <returns>An object of type T at the API.</returns>
        public virtual async Task<T> Read<T>(string uri)
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                T objectToRead = await response.Content.ReadAsAsync<T>();
                return objectToRead;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a PUT http request with the type T to the address (baseaddress + URI). 
        /// T and the URI string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object at the address (baseaddress + URI)</typeparam>
        /// <param name="uri">A URI to the API (baseaddress + uri) where objects of type T are stored.</param>
        /// <param name="toUpdate"> The type of object to update at the API.</param>
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(uri, objectToUpdate);
                response.EnsureSuccessStatusCode();
                //return await response.Content.ReadAsAsync<T>();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Sends a DELETE http request to the address (baseaddress + URI).
        /// </summary>
        /// <param name="uri">A URI to the API (baseaddress + uri) where objects of type T are stored.</param>
        public virtual async Task Delete(string uri)
        {
            try
            {
                var response = await HttpClient.DeleteAsync(uri);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

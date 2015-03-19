using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientToolBox
{
    /// <summary>
    /// <para>The class contains a static Dictionary which all instances of the class will be added to. That way one can access different httpClients without having to create them all over again (for example if login is required each time).</para>
    /// <para>Instantiatte the class with a string / uri which represents the rest api's base address for example http://driveit.azurewebsites.net/api/ </para>
    /// <para> Then when you make the CRUD calls, add the object and the rest of the uml eg "cars". If you dont know the object type, just use object </para>
    /// </summary>
    public class AwiaHttpClientToolbox
    {
        public static Dictionary<string, AwiaHttpClientToolbox> IdHttpClientMap; 
        public HttpClient HttpClient { get; set; }

        public AwiaHttpClientToolbox(string uri, AuthenticationHeaderValue authenticationHeader = null)
        {
            if(IdHttpClientMap==null) IdHttpClientMap = new Dictionary<string, AwiaHttpClientToolbox>();

            HttpClient = new HttpClient { BaseAddress = new Uri(uri) };
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationHeader != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }

            IdHttpClientMap.Add(uri,this);
        }

        public AwiaHttpClientToolbox(Uri uri, AuthenticationHeaderValue authenticationHeader = null)
        {
            if (IdHttpClientMap == null) IdHttpClientMap = new Dictionary<string, AwiaHttpClientToolbox>();

            HttpClient = new HttpClient { BaseAddress = uri };
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationHeader != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }

            IdHttpClientMap.Add(uri.ToString(), this);
        }

        /// <summary>
        /// Resets the httpclient and then sets the base address.
        /// </summary>
        /// <param name="uri"></param>
        public void SetBaseAddress(Uri uri)
        {
            HttpClient = new HttpClient { BaseAddress = uri };
        }

        /// <summary>
        /// Resets the httpclient and then sets the base address.
        /// </summary>
        /// <param name="uri"></param>
        public void SetBaseAddress(string uri)
        {
            HttpClient = new HttpClient { BaseAddress = new Uri(uri) };
        }

        /// <summary>
        /// Adds the mediatype header to the default httprequests.
        /// </summary>
        /// <param name="mediaHeaderType">For Json use: new MediaTypeWithQualityHeaderValue("application/json")</param>
        [Obsolete("SetMediaHeaders is deprecated, since only JSON is supported atm.", true)]
        public void SetMediaHeaders(MediaTypeWithQualityHeaderValue mediaHeaderType)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(mediaHeaderType);
        }

        /// <summary>
        /// Sets the authetication header to the given value. Used for API's which needs authentication.
        /// </summary>
        /// <param name="authenticationHeader"></param>
        public void SetAuthenticationHeader(AuthenticationHeaderValue authenticationHeader)
        {
            HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
        }

        /// <summary>
        /// Creates a Post httprequest with the generic T to the webAPI at the url BaseAddress + uri. 
        /// T and the uri string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object in the API at url (BaseAddress+Uri)</typeparam>
        /// <param name="uri">The uri of the api where T objects are stored</param>
        /// <param name="objectToCreate"> the object to create at the APi</param>
        /// <returns>The object which was created at the API</returns>
        public async Task<T> Create<T>(string uri, T objectToCreate)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(uri, objectToCreate);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<T>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads all the objects of type T at the webAPI on the string BaseAddress + uri. 
        /// T and the uri string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object in the API at url (BaseAddress+Uri)</typeparam>
        /// <param name="uri">The uri of the api where T objects are stored</param>
        /// <returns>All T objects in the API using the URI</returns>
        public async Task<IList<T>> ReadList<T>(string uri)
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
        /// Reads the object of type T at the webAPI on the string BaseAddress + uri. 
        /// T and the uri string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object in the API at url (BaseAddress+Uri)</typeparam>
        /// <param name="uri">The uri of the api where a single T object is stored</param>
        /// <returns>An T object in the API using the URI</returns>
        public async Task<T> Read<T>(string uri)
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
        /// Creates a Put httprequest with the generic T to the webAPI at the url BaseAddress + uri. 
        /// T and the uri string must match.
        /// </summary>
        /// <typeparam name="T"> An object matching the expected object in the API at url (BaseAddress+Uri)</typeparam>
        /// <param name="uri">The uri of the api where T objects are stored</param>
        /// <param name="objectToUpdate"> the object to update at the APi with an ID</param>
        /// <returns>A Task to await</returns>
        public async Task Update<T>(string uri, T objectToUpdate)
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(uri, objectToUpdate);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Creates a Delete httprequest to the webAPI at the url BaseAddress + uri. 
        /// </summary>
        /// <param name="uri">The uri of the API indicating a single object</param>
        /// <returns>A Task to await</returns>
        public async Task Delete<T>(string uri)
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

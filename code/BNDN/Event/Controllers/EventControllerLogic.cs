using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// EventControllerLogic is an assistant class to EventController.
    /// </summary>
    internal class EventControllerLogic
    {
        // TODO: Should this class be designed as a Singleton?
        public EventControllerLogic()
        {
            
        }


        /// <summary>
        /// Returns true when the uri (which should include a port) represents an Event.
        /// TODO: As this method does not handle a HTTP-request it should not be in this class
        /// </summary>
        /// <param name="uri">The URI to test</param>
        /// <returns>true if the Uri represents an Event, false otherwise.</returns>
        internal async Task<bool> IsEvent(Uri uri)
        {
            try
            {
                await new EventCommunicator(uri).IsIncluded();
            }
            catch (HttpRequestException)
            {
                // This means that the WebAPI didn't respond or that it failed to answer the call succesfully.
                return false;
            }
            catch (UnsupportedMediaTypeException ex)
            {
                // if this happens, something in the Web API has been changed and this method should reflect it.
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
            return true;
        }

    }
}
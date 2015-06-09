using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Server.Tests
{
    public static class HttpActionResultFlowExtensions
    {
        public static async Task<T> GetMessageContent<T>(this IHttpActionResult actionResult)
        {
            var message = await actionResult.ExecuteAsync(CancellationToken.None);

            T obj;

            message.TryGetContentValue(out obj);
            return obj;
        }
    }
}

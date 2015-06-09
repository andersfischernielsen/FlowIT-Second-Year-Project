using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Event.Tests
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

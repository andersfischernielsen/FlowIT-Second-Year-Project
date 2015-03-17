using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientToolBox.Tests
{
    class Program
    {
        private static AwiaHttpClientToolbox _testToolbox;
        static void Main(string[] args)
        {
            new AwiaHttpClientToolbox("http://jsonplaceholder.typicode.com");
            new AwiaHttpClientToolbox("http://driveit.azurewebsites.net/api/");

            GetObjects().Wait();
        }

        static private async Task GetObjects()
        {
            AwiaHttpClientToolbox.IdHttpClientMap.TryGetValue("http://driveit.azurewebsites.net/api/", out _testToolbox);
            var temp = await _testToolbox.Read<object>("cars");
            Console.WriteLine(temp);

            AwiaHttpClientToolbox.IdHttpClientMap.TryGetValue("http://jsonplaceholder.typicode.com", out _testToolbox);
            var temp2 = await _testToolbox.Read<object>("posts");
            Console.WriteLine(temp2);
        }
    }
}

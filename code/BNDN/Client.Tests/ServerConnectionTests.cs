using NUnit.Framework;

namespace Client.Tests
{
    [TestFixture]
    public class ServerConnectionTests
    {

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        // Todo: I cannot mock the HttpClientToolbox. Needs some discussion whether to integration test or make it mockable.
        public async void GetWorkflows_Ok()
        {
        }
    }
}

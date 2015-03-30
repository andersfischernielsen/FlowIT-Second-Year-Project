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
        //TODO: I cannot mock the HttpClientToolbox. Needs some discussion whether to integration test or make it mockable.
        //TODO: We might need to make methods virtual or setup an interface for this.
        public async void GetWorkflows_Ok()
        {
        }
    }
}

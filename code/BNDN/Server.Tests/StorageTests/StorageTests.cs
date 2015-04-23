using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Server.Interfaces;
using Server.Models;
using Server.Storage;

namespace Server.Tests.StorageTests
{
    [TestFixture]
    class StorageTests {
        private IServerContext _context;
        
        [SetUp]
        public void SetUp()
        {
            var context = new Mock<IServerContext>();
            context.SetupAllProperties();

            //USERS:
            var users = new List<ServerUserModel> { new ServerUserModel { Id = 1, Name = "TestingName", Password = PasswordHasher.HashPassword("TestingPassword") } };
            context.Object.Users = new FakeDbSet<ServerUserModel>(users.AsQueryable()).Object;

            //WORKFLOWS:
            var workflows = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "1", Name = "TestingName" } };
            context.Object.Workflows = new FakeDbSet<ServerWorkflowModel>(workflows.AsQueryable()).Object;

            //EVENTS:
            var events = new List<ServerEventModel> { new ServerEventModel { Id = "1", ServerWorkflowModelId = "1", Uri = "http://testing.com" } };
            context.Object.Events = new FakeDbSet<ServerEventModel>(events.AsQueryable()).Object;

            //ROLES:
            var roles = new List<ServerRoleModel> { new ServerRoleModel { Id = "1", ServerWorkflowModelId = "TestingName" } };
            context.Object.Roles = new FakeDbSet<ServerRoleModel>(roles.AsQueryable()).Object;

            //Assign the mocked StorageContext for use in tests.
            _context = context.Object;
        }

        [Test]
        public async Task TestGetUser()
        {
            var toTest = new ServerStorage(_context);
            var result = (await toTest.GetUser("TestingName", "TestingPassword"));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("TestingName", result.Name);

            Assert.IsTrue(PasswordHasher.VerifyHashedPassword("TestingPassword", result.Password));
        }

        [Test]
        public void TestLogin()
        {
            //TODO: Implement.
        }

        [Test]
        public async Task TestGetEventsFromWorkflow()
        {
            var toTest = new ServerStorage(_context);
            var result = (await toTest.GetEventsFromWorkflow(new ServerWorkflowModel { Id = "1", Name = "TestingName" })).First();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, "1");
        }

        [Test]

        public void TestAddRolesToWorkflow()
        {
            
        }

        [Test]

        public void TestAddUser()
        {
            
        }

        [Test]

        public void TestAddEventToWorkflow()
        {
            
        }

        [Test]

        public void TestUpdateEventOnWorkflow()
        {
            
        }

        [Test]

        public void TestRemoveEventFromWorkflow()
        {
            
        }

        [Test]

        public void TestGetAllWorkflows()
        {
            
        }

        [Test]

        public void TestGetWorkflow()
        {
            
        }

        [Test]

        public void TestAddNewWorkflow()
        {
            
        }

        [Test]

        public void TestUpdateWorkflow()
        {
            
        }

        [Test]

        public void TestRemoveWorkflow()
        {
            
        }

        [Test]

        public void TestRoleExists()
        {
            
        }

        [Test]

        public void TestGetRole()
        {
            
        }

    }
}

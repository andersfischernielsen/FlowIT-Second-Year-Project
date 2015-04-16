using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common;
using Moq;
using NUnit.Framework;
using Server.Models;
using Server.Storage;

namespace Server.Tests.StorageTests
{
    [TestFixture]
    class StorageTests {
        private StorageContext _context;

        [SetUp]
        public void Setup()
        {
            var context = new Mock<StorageContext>();

            //Mock USERS:
            var users = new List<ServerUserModel> { new ServerUserModel { Id = 1, Name = "TestingName" } }.AsQueryable();
            var userList = users.AsQueryable();

            context.Object.Users = new FakeDbSet<ServerUserModel>(userList).Object;

            //Mock WORKFLOWS:
            var workflows = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "1", Name = "TestingName" } }.AsQueryable();
            var workflowList = workflows.AsQueryable();
            context.Object.Workflows = new FakeDbSet<ServerWorkflowModel>(workflowList).Object;

            //EVENTS:
            var events = new List<ServerEventModel> { new ServerEventModel { Id = "1", ServerWorkflowModelId = "1", Uri = "http://testing.com" } }.AsQueryable();
            var eventList = events.AsQueryable();
            context.Object.Events = new FakeDbSet<ServerEventModel>(eventList).Object;

            //ROLES:
            var roles = new List<ServerRoleModel> { new ServerRoleModel { Id = "1", ServerWorkflowModelId = "TestingName" } }.AsQueryable();
            var rolesList = roles.AsQueryable();
            context.Object.Roles = new FakeDbSet<ServerRoleModel>(rolesList).Object;

            //Assign the mocked StorageContext for use in tests.
            _context = context.Object;
        }

        private class FakeDbSet<T> where T : class {

            public DbSet<T> Object { get; private set; }

            public FakeDbSet(IQueryable<T> list)
            {
                //Below is a bunch of crazy-looking code, which enables us to use a list as a DbSet. 
                //Credit to http://www.loganfranken.com/blog/517/mocking-dbset-queries-in-ef6/ for figuring this out.
                //This is done to validate what the ServerStorage instance does to the (fake) database.

                // Force DbSet to return the IQueryable members of our converted list object as its data source.
                var mockSet = new Mock<DbSet<T>>();
                mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(list.Provider);
                mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(list.Expression);
                mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(list.ElementType);
                mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(list.GetEnumerator());

                Object = mockSet.Object;
            }
        }

        [Test]
        public void TestGetUser()
        {
            var toTest = new ServerStorage(_context);
            var result = (toTest.GetUser("TestingName"));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("TestingName", result.Name);
        }

        [Test]
        public void TestLogin()
        {
            //TODO: Implement.
        }

        [Test]
        public void TestGetEventsFromWorkflow()
        {
            var toTest = new ServerStorage(_context);
            var result = (toTest.GetEventsFromWorkflow( new ServerWorkflowModel { Id = "1", Name = "TestingName" } )).First();

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

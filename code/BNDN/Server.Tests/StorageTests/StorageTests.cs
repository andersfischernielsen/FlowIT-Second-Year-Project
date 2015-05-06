using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Server.Exceptions;
using Server.Interfaces;
using Server.Models;
using Server.Storage;

namespace Server.Tests.StorageTests
{
    [TestFixture]
    class StorageTests 
    {
        private IServerContext _context;
        private List<ServerUserModel> users;
        private List<ServerWorkflowModel> workflows;
        private List<ServerEventModel> events;
        private List<ServerRoleModel> roles;

        [SetUp]
        public void SetUp()
        {
            var context = new Mock<IServerContext>(MockBehavior.Strict);
            context.SetupAllProperties();

            //USERS:
            users = new List<ServerUserModel> { new ServerUserModel { Name = "TestingName", Password = PasswordHasher.HashPassword("TestingPassword") } };
            var fakeUserSet = new FakeDbSet<ServerUserModel>(users.AsQueryable()).Object;

            //WORKFLOWS:
            workflows = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "1", Name = "TestingName" } };
            var fakeWorkflowsSet = new FakeDbSet<ServerWorkflowModel>(workflows.AsQueryable()).Object;

            //EVENTS:
            events = new List<ServerEventModel> { new ServerEventModel { Id = "1", ServerWorkflowModelId = "1", Uri = "http://testing.com", ServerWorkflowModel = new ServerWorkflowModel() { Id = "1", Name = "TestingName" } } };
            var fakeEventsSet = new FakeDbSet<ServerEventModel>(events.AsQueryable()).Object;

            //ROLES:
            roles = new List<ServerRoleModel> { new ServerRoleModel { Id = "1", ServerWorkflowModelId = "TestingName" } };
            var fakeRolesSet = new FakeDbSet<ServerRoleModel>(roles.AsQueryable()).Object;

            // Final prep
            context.Setup(m => m.Users).Returns(fakeUserSet);
            context.Setup(m => m.Workflows).Returns(fakeWorkflowsSet);  
            context.Setup(m => m.Events).Returns(fakeEventsSet);
            context.Setup(m => m.Roles).Returns(fakeRolesSet);

            context.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

            //Assign the mocked StorageContext for use in tests.
            _context = context.Object;
        }

        #region AddEventToWorkflow

        [Test]
        public void AddEventToWorkflow_HandlesNullArgument()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            ServerEventModel nullArgument = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddEventToWorkflow(nullArgument));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void AddEventToWorkflow_WhenWorkflowDoesNotExistNotFoundExceptionIsRaised()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            var eventModel = new ServerEventModel()
            {
                ServerWorkflowModelId = "DailyCleaning",
            };
            
            // Act
            var testDelegate = new TestDelegate(async() => await storage.AddEventToWorkflow(eventModel));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [Test]
        public void AddEventToWorkflow_WhenEventAlreadyExistsAnEventExistsExceptionIsRaised()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            var eventModel = new ServerEventModel()
            {
                ServerWorkflowModelId = "1",
                Id = "1"
            };

            // Act
            var testDelegate = new TestDelegate(async() => await storage.AddEventToWorkflow(eventModel));

            // Assert
            Assert.Throws<EventExistsException>(testDelegate);
        }

        [Test]
        public void AddEventToWorkflow_HandlesIllegalStorageStateCorrectlyByThrowingException()
        {
            // Arrange
            // Copies Setup() method - only difference is in WORKLOWS, where an extra entry is inserted with identical ID as first entry
            var context = new Mock<IServerContext>();
            context.SetupAllProperties();

            //USERS:
            var users = new List<ServerUserModel> { new ServerUserModel { Name = "TestingName", Password = PasswordHasher.HashPassword("TestingPassword") } };
            var fakeUsersSet = new FakeDbSet<ServerUserModel>(users.AsQueryable()).Object;

            //WORKFLOWS:
            var workflows = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "1", Name = "TestingName" }, new ServerWorkflowModel() { Id = "1", Name = "ConflictingWorkflowDueToIdenticalId" } };
            context.Object.Workflows = new FakeDbSet<ServerWorkflowModel>(workflows.AsQueryable()).Object;

            //EVENTS:
            var events = new List<ServerEventModel> { new ServerEventModel { Id = "1", ServerWorkflowModelId = "1", Uri = "http://testing.com", ServerWorkflowModel = new ServerWorkflowModel() { Id = "1", Name = "TestingName" } } };
            context.Object.Events = new FakeDbSet<ServerEventModel>(events.AsQueryable()).Object;

            //ROLES:
            var roles = new List<ServerRoleModel> { new ServerRoleModel { Id = "1", ServerWorkflowModelId = "TestingName" } };
            context.Object.Roles = new FakeDbSet<ServerRoleModel>(roles.AsQueryable()).Object;

            //Assign the mocked StorageContext for use in tests.
            _context = context.Object;

            IServerStorage storage = new ServerStorage(_context);
            var eventModel = new ServerEventModel()
            {
                ServerWorkflowModelId = "1",
                Id = "32"
            };

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddEventToWorkflow(eventModel));

            // Assert
            Assert.Throws<IllegalStorageStateException>(testDelegate);
        }

        [Test]
        public void AddEventToWorkflow_WillAddEventToWorkflow()
        {
            // TODO: Not implemented yet <- Implement!
            Assert.Fail();
        }

        #endregion

        #region AddNewWorkflow

        [Test]
        public void AddNewWorkflow_HandlesNullArgumentCorrectly()
        {
            // Arrange 
            IServerStorage storage = new ServerStorage(_context);
            ServerWorkflowModel nullArgument = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddNewWorkflow(nullArgument));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void AddNewWorkflow_WhenWorkflowExistsExceptionIsRaised()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            var workflowModel = new ServerWorkflowModel {Id = "1", Name = "TestingName"};

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddNewWorkflow(workflowModel));

            // Assert
            Assert.Throws<WorkflowAlreadyExistsException>(testDelegate);
        }

        [Test]
        public async Task AddNewWorkflow_WillAddWorkflow()
        {
            // TODO: Implement this one
            Assert.Fail();
        }
        #endregion

        #region AddRolesToUser
        [TestCase(null)]
        [TestCase("Per")]
        public void AddRolesToUser_HandlesNullArguments(string username)
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            
            IEnumerable<ServerRoleModel> roles = null;
            if (username == null)   // If one argument is null, the other should not be. 
            {
                roles = new List<ServerRoleModel>
                {
                    new ServerRoleModel {Id = "Ambassador"},
                    new ServerRoleModel {Id = "Professor"}
                };
            }
            

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddRolesToUser(username, roles));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void AddRolesToUser_WhenUserDoesNotExistNotFoundExceptionIsThrown()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            IEnumerable<ServerRoleModel> roles = new List<ServerRoleModel>
                {
                    new ServerRoleModel {Id = "Ambassador"},
                    new ServerRoleModel {Id = "Professor"}
                };
            var username = "NonExistingUser";

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddRolesToUser(username, roles));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [Test]
        public void AddRolesToUser_AddsRolesToUser()
        {
            // TODO: How to?
            Assert.Fail();
        }

        [Test]
        public void AddRolesToUser_DoesNotCreateDuplicateRolesWhenProvidedAlreadyExistingRoles()
        {
            // TODO: How to?
            Assert.Fail();
        }
        #endregion

        #region AddRolesToWorkflow

        [Test]
        public void AddRolesToWorkflow_HandlesNullArgument()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            IEnumerable<ServerRoleModel> nullArgument = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddRolesToWorkflow(nullArgument));
            
            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }


        #endregion

        #region AddUser

        [Test]
        public void AddUser_HandlesNullArgument()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            ServerUserModel nullArgument = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddUser(nullArgument));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void AddUser_WhenUserAlreadyExistsUserExistsExceptionIsThrown()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            ServerUserModel userToAdd = new ServerUserModel {Name = "TestingName"};

            // Act
            var testDelegate = new TestDelegate(async () => await storage.AddUser(userToAdd));

            // Assert
            Assert.Throws<UserExistsException>(testDelegate);
        }

        [Test]
        public async Task AddUser_WillStoreProvidedPasswordInADifferentRepresentation()
        {
            // TODO: Help Morten implement this!
            // This test checks, that the provided password is not stored in its original representation in the database.

            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            var inputPassword = "JellyBeans";
            var name = "George";
            var userToAdd = new ServerUserModel {Name = name, Password = inputPassword};

            // Act
            await storage.AddUser(userToAdd);

            // Assert
            var georgeInContext = _context.Users.SingleOrDefault(x => x.Name == name);

            if (georgeInContext == null)
            {
                Assert.Fail();
            }

            Assert.AreNotEqual(inputPassword,georgeInContext.Password);
        }

        [Test]
        public void AddUser_WillAddUser()
        {
            Assert.Fail();
        }
        #endregion

        #region EventExists

        [TestCase(null,null)]
        [TestCase("GenericWorkflow",null)]
        [TestCase(null,"Register patient")]
        public void EventExists_HandlessNullArguments(string workflowId, string eventId)
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async() => await storage.EventExists(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [TestCase("1","1",true)]
        [TestCase("1","2",false)]
        [TestCase("2","1",false)]
        [TestCase("","",false)]
        public async Task EventExists_Test(string workflowId,string eventId, bool expectedResult)
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var actualResult = await storage.EventExists(workflowId,eventId);

            // Assert
            Assert.AreEqual(expectedResult,actualResult);
        }
        #endregion

        #region GetAllWorkflows

        [Test]
        public async Task GetAllWorkflows_CanHandleThatNoWorkflowExists()
        {
            // Arrange
            var context = new Mock<IServerContext>(MockBehavior.Strict);
            context.SetupAllProperties();

            //USERS:
            users = new List<ServerUserModel> { new ServerUserModel { Name = "TestingName", Password = PasswordHasher.HashPassword("TestingPassword") } };
            var mockSetUsers = new FakeDbSet<ServerUserModel>(users.AsQueryable());

            //WORKFLOWS:
            workflows = new List<ServerWorkflowModel> ();       // This is the difference from the Setup() method
            var mockSetWorkflows = new FakeDbSet<ServerWorkflowModel>(workflows.AsQueryable());

            //EVENTS:
            events = new List<ServerEventModel> { new ServerEventModel { Id = "1", ServerWorkflowModelId = "1", Uri = "http://testing.com", ServerWorkflowModel = new ServerWorkflowModel() { Id = "1", Name = "TestingName" } } };
            var mockSetEvents = new FakeDbSet<ServerEventModel>(events.AsQueryable());

            //ROLES:
            roles = new List<ServerRoleModel> { new ServerRoleModel { Id = "1", ServerWorkflowModelId = "TestingName" } };
            var mockSetRoles = new FakeDbSet<ServerRoleModel>(roles.AsQueryable());

            // Final prep
            context.Setup(m => m.Users).Returns(mockSetUsers.Object);
            context.Setup(m => m.Workflows).Returns(mockSetWorkflows.Object);
            context.Setup(m => m.Events).Returns(mockSetEvents.Object);
            context.Setup(m => m.Roles).Returns(mockSetRoles.Object);

            context.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

            //Assign the mocked StorageContext for use in tests.
            _context = context.Object;

            IServerStorage storage = new ServerStorage(_context);

            // Act
            var workflowList = await storage.GetAllWorkflows();

            // Assert
            Assert.IsEmpty(workflowList);
        }
        #endregion

        #region GetEventsFromWorkflow

        [Test]
        public void GetEventsFromWorkflow_HandlesNullArgument()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            string nullWorkflowId = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetEventsFromWorkflow(nullWorkflowId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void GetEventsFromWorkflow_WhenWorkflowDoesNotExistExceptionIsRaised()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetEventsFromWorkflow("NonExistingWorkflowId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [Test]
        public async Task GetEventsFromWorkflow_WhenWorkflowExistChildEventsAreReturned()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var eventsList = await storage.GetEventsFromWorkflow("1");
            var singleEventBelongingToWorkflow1 = eventsList.First();

            // Assert
            Assert.AreEqual(1,eventsList.Count());
            Assert.AreEqual("1",singleEventBelongingToWorkflow1.Id);
            Assert.AreEqual("1", singleEventBelongingToWorkflow1.ServerWorkflowModelId);
            Assert.AreEqual("http://testing.com",singleEventBelongingToWorkflow1.Uri);
            Assert.AreEqual("1",singleEventBelongingToWorkflow1.ServerWorkflowModel.Id);
            Assert.AreEqual("TestingName",singleEventBelongingToWorkflow1.ServerWorkflowModel.Name);
        }

        #endregion

        #region GetHistoryForWorkflow

        [Test]
        public void GetHistoryForWorkflow_HandlesNullArgument()
        {
            // Arrange
            IServerHistoryStorage storage = new ServerStorage(_context);
            string nullWorkflowId = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetHistoryForWorkflow(nullWorkflowId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void GetHistoryForWorkflow_WhenWorkflowDoesNotExistNotFoundExceptionIsRaised()
        {
            // Arrange
            IServerHistoryStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetHistoryForWorkflow("NonExistingWorkflowId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
        #endregion

        #region GetRole
        [TestCase(null, null)]
        [TestCase("Doctor", null)]
        [TestCase(null, "Healtchcare")]
        public void GetRole_HandlesNullArgument(string rolename, string workflowId)
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetRole(rolename,workflowId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void GetRole_WhenWorkflowDoesNotExistNotFoundExceptionIsRaised()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetRole("Patient","nonexistingworkflowid"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [Test]
        public void GetRole_GetsRole()
        {
            Assert.Fail();
        }
        #endregion

        #region GetUser
        [Test]
        public async Task GetUser()
        {
            var toTest = new ServerStorage(_context);
            var result = (await toTest.GetUser("TestingName", "TestingPassword"));

            Assert.IsNotNull(result);
            Assert.AreEqual("TestingName", result.Name);

            Assert.IsTrue(PasswordHasher.VerifyHashedPassword("TestingPassword", result.Password));
        }

        [Test]
        public void GetUser_HandlesEmptyStringsForInput()
        {
            // Arrange
            var toTest = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await toTest.GetUser("", ""));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public async Task GetUser_ReturnsNullWhenUserDoesNotExist()
        {
            // Arrange
            var user = "Spock";
            var password = "MSEnterprise";
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var result = await storage.GetUser(user, password);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUser_ReturnsNonNullWhenUserExists()
        {
            // Arrange
            var user = "TestingName";
            var password = "TestingPassword";
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var result = await storage.GetUser(user, password);

            // Assert
            Assert.IsNotNull(result);
        }
        #endregion

        #region GetWorkflow

        [Test]
        public void GetWorkflow_HandlessNullArgument()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);
            string nullWorkflowId = null;

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetWorkflow(nullWorkflowId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void GetWorkflow_WhenWorkflowDoesNotExistNotFoundExceptionIsThrown()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var testDelegate = new TestDelegate(async () => await storage.GetWorkflow("NonexistingWorkflow"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [Test]
        public async Task GetWorkflow_WhenWorkflowExistsWorkflowIsReturned()
        {
            // Arrange
            IServerStorage storage = new ServerStorage(_context);

            // Act
            var actualWorkflow = await storage.GetWorkflow("1");

            var workflowList =
                from w in workflows
                where w.Id == "1"
                select 1;

            var expectedWorkflow = new ServerWorkflowModel
            {
                Id = "1",
                Name = "TestingName"
            };

            // Assert
            Assert.AreEqual(expectedWorkflow.Id,actualWorkflow.Id);
            Assert.AreEqual(expectedWorkflow.Name,actualWorkflow.Name);
        }
        #endregion

        #region Login

        #endregion








        [Test]
        public void TestLogin()
        {
        }

        [Test]
        public async Task TestGetEventsFromWorkflow()
        {
            var toTest = new ServerStorage(_context);
            var result = (await toTest.GetEventsFromWorkflow("1")).First();

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

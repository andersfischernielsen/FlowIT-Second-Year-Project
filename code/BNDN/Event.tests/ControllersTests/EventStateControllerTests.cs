//using System.Collections.Generic;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Common;
//using Event.Controllers;
//using Event.Interfaces;
//using Event.Logic;
//using Event.Models;
//using Event.Storage;
//using Moq;
//using NUnit.Framework;

//namespace Event.tests
//{
//    [TestFixture]
//    class EventStateControllerTests
//    {
//        private StateController _stateController;
//        private IStateLogic _stateLogic;
//        private IEventStorage _storage;
//        private EventAddressDto _eventAddressDto;

//        [SetUp]
//        public void Setup()
//        {


//            var storage = new Mock<IEventStorage>();
//            storage.SetupAllProperties();

//            storage.Setup(m => m.ClearLock(It.IsAny<string>())).Callback(() => storage.Object.SetLockDto(It.IsAny<string>(), null));

//            _storage = storage.Object;

//            var locking = new Mock<ILockingLogic>();

//            var auth = new Mock<IAuthLogic>();

//            _stateLogic = new StateLogic(storage.Object, locking.Object, auth.Object);
//            _eventAddressDto = new EventAddressDto(){Id = "Lock"};

//            storage.Object.SetEve

//            _stateLogic.EventId = "1";
//            _stateLogic.WorkflowId = "2";
//            _stateLogic.Name = "TestEvent";
//            _stateLogic.Pending = true;
//            _stateLogic.Executed = true;
//            _stateLogic.Included = false;
//            _stateLogic.Roles = new List<string>{"TEACHER"};
//            _stateLogic.LockDto = new LockDto(){LockOwner = "Lock"};
//            _stateLogic.Inclusions = new HashSet<RelationToOtherEventModel>();
//            _stateLogic.Responses = new HashSet<RelationToOtherEventModel>();
//            _stateLogic.Conditions = new HashSet<RelationToOtherEventModel>();
//            _stateLogic.Exclusions = new HashSet<RelationToOtherEventModel>();
//            //_stateLogic.IsExecutable();
//            //_stateLogic.EventStateDto = ?

//            _stateController = new StateController(_stateLogic);
//        }

//        #region GET-tests

//        [Test]
//        public void TestGetExecutedReturnsTrue()
//        {
//            //Act
//            var result = _stateController.GetExecuted(_eventAddressDto.Id, "EventId");

//            //Assert
//            Assert.AreEqual(true, result);
//        }

//        [Test]
//        public void TestGetIncludedReturnsFalse()
//        {
//            //Act
//            var result = _stateController.GetIncluded(_eventAddressDto.Id,"EventId");

//            //Assert
//            Assert.AreEqual(false, result);
//        }
//        #endregion

//        #region PUT-tests

//        [Test]
//        public async Task TestExecute()
//        {
//            await _stateLogic.SetIncluded("eventId", "SenderId", true);
//            //Test execution of event with a given role.
//            await _stateController.Execute(new ExecuteDto {Roles = new List<string> {"TEACHER"}},"SenderId");
//            Assert.IsTrue(await _stateLogic.IsExecuted("eventId", "SenderId"));

//            //TODO: Test the rest of Execute()...
//        }

//        [Test]
//        public async Task TestPutPendingReturnsFalse()
//        {
//            //Act
//            await _stateController.UpdatePending("SenderId", false, _eventAddressDto);

//            //Assert
//            Assert.AreEqual(false, _storage.GetPending("eventId"));
//        }

//        [Test]
//        public async Task TestPutIncludedReturnsTrue()
//        {
//            //Act
//            await _stateController.UpdateIncluded("SenderId", true, _eventAddressDto);

//            //Assert
//            Assert.IsTrue(await _storage.GetIncluded("eventId"));
//        }
//        #endregion

//        #region POST-tests

//        [Test]
//        public void TestPostLockUpdatesLockOwner()
//        {
//            //Arrange
//            LockDto testLock = new LockDto() {LockOwner = "1"};

//            //Act
//            _stateLogic.LockDto = null;
//            _stateController.Lock(testLock, "eventId");

//            //Assert
//            Assert.AreEqual(testLock, _stateLogic.LockDto);
//        }
//        #endregion

//        #region DELETE-tests

//        [Test]
//        public void TestUnlocking()
//        {
//            //Act
//            _stateController.Unlock(_eventAddressDto.Id, "eventId");

//            //Assert
//            Assert.AreEqual(null, _stateLogic.LockDto);

//        }

//        #endregion

//    }
//}

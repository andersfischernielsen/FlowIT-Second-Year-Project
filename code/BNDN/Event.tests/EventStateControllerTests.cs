using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Common;
using Event.Controllers;
using Event.Models;
using Event.Storage;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    class EventStateControllerTests
    {
        private EventStateController _eventStateController;
        private EventLogic _eventLogic;
        private EventAddressDto _eventAddressDto;

        [SetUp]
        public void Setup()
        {
            _eventStateController = new EventStateController();
            _eventLogic = new EventLogic();
            _eventAddressDto = new EventAddressDto(){Id = "Lock"};

            _eventLogic.EventId = "1";
            _eventLogic.WorkflowId = "2";
            _eventLogic.Pending = true;
            _eventLogic.Executed = true;
            _eventLogic.Included = false;
            _eventLogic.Role = "TEACHER";
            _eventLogic.LockDto = new LockDto(){LockOwner = "Lock"};
            //_eventLogic.IsExecutable();
            //_eventLogic.EventStateDto = ?
        }

        #region GET-tests
        [Test]
        public void TestGetPendingReturnsTrue()
        {
            //Act
            var result = _eventStateController.GetPending(_eventAddressDto.Id);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestGetExecutedReturnsTrue()
        {
            //Act
            var result = _eventStateController.GetExecuted(_eventAddressDto.Id);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestGetIncludedReturnsFalse()
        {
            //Act
            var result = _eventStateController.GetIncluded((_eventAddressDto.Id));

            //Assert
            Assert.AreEqual(false, result);
        }
        #endregion

        #region PUT-tests

        [Test]
        public async void TestExecute()
        {
            //Test execution of event with a given role.
            await _eventStateController.Execute(new ExecuteDto {Roles = new List<string> {"TEACHER"}});
            Assert.IsTrue(_eventLogic.Executed);

            //TODO: Test the rest of Execute()...
        }

        [Test]
        public void TestPutPendingReturnsFalse()
        {
            //Act
            _eventStateController.UpdatePending(_eventAddressDto, false);
            var result = _eventLogic.Pending;

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestPutIncludedReturnsTrue()
        {
            //Act
            _eventStateController.UpdateIncluded(_eventAddressDto, true);
            var result = _eventLogic.Included;

            //Assert
            Assert.AreEqual(true, result);
        }
        #endregion

        #region POST-tests

        [Test]
        public void TestPostLockUpdatesLockOwner()
        {
            //Arrange
            LockDto testLock = new LockDto() {LockOwner = "1"};

            //Act
            _eventLogic.LockDto = null;
            _eventStateController.Lock(testLock);

            //Assert
            Assert.AreEqual(testLock, _eventLogic.LockDto);
        }
        #endregion

        #region DELETE-tests

        [Test]
        public void TestUnlocking()
        {
            //Act
            _eventStateController.Unlock(_eventAddressDto.Id);

            //Assert
            Assert.AreEqual(null, _eventLogic.LockDto);

        }

        #endregion

    }
}

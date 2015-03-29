using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;
using Event.Models;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    class EventLogicTests
    {
        [Test]
        public void SetupLogicStorageIsNotNullTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();

            //Act
            //Assert
            Assert.IsNotNull(eventLogic.Storage);
        }

        [Test]
        public void SetupLogicIsNotNullTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();

            //Act

            //Assert
            Assert.IsNotNull(eventLogic);
        }

        #region Rulehandling
        [Test]
        public void IsExecutableReturnsTrueWhenNoConditionsTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Included = true;

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(true, result);
        }
        [Test]
        public void IsExecutableReturnsFalseWhenNoConditionsTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Included = false;

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateRulesFailDueToRulesBeingNullTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Storage.EventUris.Add("Test", new Uri("http://test/"));

            //Act
            try
            {
                eventLogic.UpdateRules("Test", null).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Value cannot be null.\r\nParameter name: rules", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        //This fails due to incorrect code.
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void UpdateRulesFailDueToIdNotExistingTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();

            //Act
            try
            {
                eventLogic.UpdateRules("Test", new EventRuleDto()).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Nonexistent id", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        public void UpdateRulesRunsTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Storage.EventUris.Add("Test", new Uri("http://test/"));

            //Act
            eventLogic.UpdateRules("Test", new EventRuleDto()).Wait();

            //Assert
        }

        [Test]
        public void UpdateRuleAddsValueToCollectionTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Conditions = new HashSet<Uri>();
            Uri uri = new Uri("http://test/");
            eventLogic.Storage.EventUris.Add("Test", uri);

            //Act
            eventLogic.UpdateRules("Test", new EventRuleDto() {Condition = true}).Wait();
            var result = eventLogic.Conditions.Contains(uri);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void UpdateRuleRemovesValueFromCollectionTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Conditions = new HashSet<Uri>();
            Uri uri = new Uri("http://test/");
            eventLogic.Storage.EventUris.Add("Test", uri);
            eventLogic.Conditions.Add(uri);

            //Act
            eventLogic.UpdateRules("Test", new EventRuleDto(){Condition = false}).Wait();
            var result = eventLogic.Conditions.Contains(uri);

            //Assert
            Assert.AreEqual(false, result);
        }
        #endregion

        #region DTO Creation

        [Test]
        public void GetEventStateDtoWhenIncludedIsFalseTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Included = false;
            eventLogic.Executed = true;
            eventLogic.Pending = true;

            //Act
            var result = eventLogic.EventStateDto.Result;

            //Assert
            Assert.AreEqual(false,result.Included);
            Assert.AreEqual(true, result.Executed);
            Assert.AreEqual(true, result.Pending);
            Assert.AreEqual(false, result.Executable);

            // We don't know if ID and Name should be null
            Assert.AreEqual(null, result.Name);
            Assert.AreEqual(null, result.Id);
        }
        [Test]
        public void GetEventStateDtoWhenIncludedIsTrueTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Included = true;
            eventLogic.Executed = false;
            eventLogic.Pending = false;
          
            //Act
            var result = eventLogic.EventStateDto.Result;

            //Assert
            Assert.AreEqual(true, result.Included);
            Assert.AreEqual(false, result.Executed);
            Assert.AreEqual(false, result.Pending);
            Assert.AreEqual(true, result.Executable);

            // We don't know if ID and Name should be null
            Assert.AreEqual(null, result.Name);
            Assert.AreEqual(null, result.Id);
        }
        #endregion

        [Test]
        public void GetEventDtoPropertyTest()
        {
            //Arrange
            var eventLogic = EventLogic.GetState();
            eventLogic.Name = "TestName";
            eventLogic.EventId = "TestId";
            eventLogic.WorkflowId = "TestWId";
            eventLogic.Included = true;
            eventLogic.Executed = false;
            eventLogic.Pending = false;

            //Act
            var result = eventLogic.EventDto.Result;

            //Assert
            Assert.AreEqual(true, result.Included);
            Assert.AreEqual(false, result.Executed);
            Assert.AreEqual(false, result.Pending);
            Assert.AreEqual("TestWId", result.WorkflowId);
            Assert.AreEqual("TestName", result.Name);
            Assert.AreEqual("TestId", result.EventId);
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.DTO.History;
using Moq;
using NUnit.Framework;
using Server.Interfaces;
using Server.Logic;

namespace Server.Tests.LogicTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    class WorkflowsHistoryLogicTests
    {
        private Mock<IServerHistoryStorage> _storageMock;
        private List<HistoryModel> _testModelList;
        private IWorkflowHistoryLogic _toTest;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var mock = new Mock<IServerHistoryStorage>();

            mock.Setup(m => m.GetHistoryForWorkflow(It.IsAny<string>()))
                .Returns((string workflowId) => 
                            Task.Run( () => _testModelList.Where(w => w.WorkflowId == workflowId).AsQueryable() )).Verifiable();

            mock.Setup(m => m.SaveHistory(It.IsAny<HistoryModel>()))
                .Returns((HistoryModel model) =>
                            Task.Run( () => _testModelList.Add(model))).Verifiable();

            mock.Setup(m => m.SaveNonWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Returns((HistoryModel model) => 
                            Task.Run(() => _testModelList.Add(model))).Verifiable();

            mock.Setup(m => m.Dispose()).Verifiable();

            _storageMock = mock;
            _toTest = new WorkflowHistoryLogic(mock.Object);
        }

        [SetUp]
        public void ResetList()
        {
            _testModelList = new List<HistoryModel>();
        }

        [Test]
        public async void GetHistoryForWorkflowTest()
        {
            //Setup.
            var testHistory = CreateTestHistory();

            _testModelList.Add(testHistory);

            //Execute.
            var collection = await _toTest.GetHistoryForWorkflow(@"&%¤#æøå*¨^´`?");
            var result = collection.First();

            //Assert.
            _storageMock.Verify(m => m.GetHistoryForWorkflow(It.IsAny<string>()), Times.Once);
            Assert.Throws<ArgumentNullException>(async () => await _toTest.GetHistoryForWorkflow(null));
            Assert.DoesNotThrow(async () => await _toTest.GetHistoryForWorkflow(@"&%¤#æøå*¨^´`?"));
            Assert.IsTrue(_testModelList.Any());
            Assert.AreEqual(testHistory.WorkflowId, result.WorkflowId);
            Assert.AreEqual(testHistory.EventId, result.EventId);
            Assert.AreEqual(testHistory.HttpRequestType, result.HttpRequestType);
            Assert.AreEqual(testHistory.Message, result.Message);
            Assert.AreEqual("01/01/0001 00:00:00", result.TimeStamp);
            Assert.AreEqual(testHistory.MethodCalledOnSender, result.MethodCalledOnSender);
        }

        [Test]
        public void SaveHistoryTest()
        {
            //Setup.
            var testHistory = CreateTestHistory();

            //Execute.
            Assert.DoesNotThrow(async () => await _toTest.SaveHistory(testHistory));

            //Assert.
            Assert.Throws<ArgumentNullException>(async () => await _toTest.SaveHistory(null));
            _storageMock.Verify(m => m.SaveHistory(It.IsAny<HistoryModel>()), Times.Once);
            Assert.IsTrue(_testModelList.Any());
        }

        public void SaveNoneWorkflowSpecificHistoryTest()
        {
            //Setup.
            var testHistory = CreateTestHistory();

            //Execute.
            Assert.DoesNotThrow(async () => await _toTest.SaveNoneWorkflowSpecificHistory(testHistory));

            //Assert.
            Assert.Throws<ArgumentNullException>(async () => await _toTest.SaveNoneWorkflowSpecificHistory(null));
            _storageMock.Verify(m => m.SaveNonWorkflowSpecificHistory(It.IsAny<HistoryModel>()), Times.Once);
            Assert.IsTrue(_testModelList.Any());
        }

        [Test]
        public void DisposeTest()
        {
            using (_toTest)
            {
                Assert.IsTrue(true);
            }

            _storageMock.Verify(m => m.Dispose(), Times.Once);
        }

        private static HistoryModel CreateTestHistory()
        {
            return new HistoryModel
            {
                EventId = @"&%¤#æøå*¨^´`?",
                WorkflowId = @"&%¤#æøå*¨^´`?",
                HttpRequestType = @"&%¤#æøå*¨^´`?",
                Message = @"&%¤#æøå*¨^´`?",
                MethodCalledOnSender = @"&%¤#æøå*¨^´`?",
                TimeStamp = DateTime.MinValue
            };
        }
    }
}

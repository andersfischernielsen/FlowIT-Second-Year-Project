using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO.History;
using Moq;
using NUnit.Framework;
using Server.Interfaces;
using Server.Logic;

namespace Server.Tests.LogicTests
{
    [TestFixture]
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

            _storageMock = mock;
            _toTest = new WorkflowHistoryLogic(mock.Object);
        }

        [SetUp]
        public void ResetList()
        {
            _testModelList = new List<HistoryModel>();
        }

        [Test]
        public void GetHistoryForWorkflowTest()
        {
            //Setup.
            var testHistory = new HistoryModel
            {
                EventId = @"&%¤#æøå*¨^´`?",
                WorkflowId = @"&%¤#æøå*¨^´`?",
                HttpRequestType = @"&%¤#æøå*¨^´`?",
                Message = @"&%¤#æøå*¨^´`?",
                MethodCalledOnSender = @"&%¤#æøå*¨^´`?",
                TimeStamp = DateTime.MinValue
            };

            _testModelList.Add(testHistory);

            //Execute.
            var result = _toTest.GetHistoryForWorkflow(@"&%¤#æøå*¨^´`?");

            //Assert.
            _storageMock.Verify(m => m.GetHistoryForWorkflow(It.IsAny<string>()), Times.Once);
            Assert.IsTrue(_testModelList.Any());
            Assert.AreEqual(testHistory, _testModelList.First());
        }

        public void SaveHistoryTest()
        {

        }

        public void SaveNoneWorkflowSpecificHistoryTest()
        {

        }

        public void DisposeTest()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Server.Models;
using Server.Storage;

namespace Server.Tests
{
    [TestFixture]
    public class ServerStorageTest
    {

        [SetUp]
        public void Setup()
        {
            var dummy1 = new ServerWorkflowModel() {Name = "Pay rent", WorkflowId = "1"};
            var dummy2 = new ServerWorkflowModel() { Name = "How to get good grades", WorkflowId = "2" };
            var list = new List<ServerWorkflowModel>() { dummy1, dummy2 };

            var mock = new Mock<IServerStorage>();
            mock.Setup(x => x.GetAllWorkflows()).Returns(list);
        }


        [Test]
        public void TestMethod1()
        {

        }
    }
}

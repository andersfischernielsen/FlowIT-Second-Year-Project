using System;
using System.Collections.Generic;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Server.Storage;

namespace Server.Tests
{
    [TestFixture]
    public class ServerStorageTest
    {

        [SetUp]
        public void Setup()
        {
            var dummy1 = new WorkflowDto() {Name = "Pay rent", Id = "1"};
            var dummy2 = new WorkflowDto() {Name = "How to get good grades", Id = "2"};
            var list = new List<WorkflowDto>() {dummy1, dummy2};

            var mock = new Mock<IServerStorage>();
            mock.Setup(x => x.GetAllWorkflows()).Returns(list);
        }


        [Test]
        public void TestMethod1()
        {

        }
    }
}

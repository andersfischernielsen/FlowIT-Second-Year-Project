using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Server.Models;
using Server.Storage;

namespace Server.Tests
{
    [TestFixture]
    class Temp
    {
        private ServerStorage _s;

        [Test]
        public void Test1()
        {
            _s = new ServerStorage();
            var wm = new ServerWorkflowModel
            {
                Name = "Test2",
                ID = "1",
                ServerEventModels = new List<ServerEventModel>(),
                ServerRolesModels = new List<ServerRolesModel>()
            };
            _s.AddNewWorkflow(wm);
        }

        [Test]
        public void Test2()
        {
            _s = new ServerStorage();
            var v = _s.GetWorkflow("1");
            _s.RemoveWorkflow(v);
        }

        [Test]
        public void Test3()
        {
            _s = new ServerStorage();
            var wm = new ServerWorkflowModel
            {
                Name = "Test2",
                ID = "1",
                ServerEventModels = new List<ServerEventModel>(),
                ServerRolesModels = new List<ServerRolesModel>()
            };
            _s.AddNewWorkflow(wm);

            var e = new ServerEventModel()
            {
                ID = "Adam",
                ServerWorkflowModelID = "1",
                Uri = "http://www.google.dk",
                ServerWorkflowModel = _s.GetWorkflow("1")
            };

            _s.AddEventToWorkflow(e);
        }

        [Test]
        public void Test4()
        {
            _s = new ServerStorage();
            _s.RemoveEventFromWorkflow(_s.GetWorkflow("1"), "Adam");
        }
    }
}

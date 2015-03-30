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
        private ServerStorage s;

        [Test]
        public void Test1()
        {
            s = new ServerStorage();
            var wm = new ServerWorkflowModel
            {
                Name = "Test",
                ID = "2",
                ServerEventModels = new List<ServerEventModel>()
            };
            s.AddNewWorkflow(wm);
        }
    }
}

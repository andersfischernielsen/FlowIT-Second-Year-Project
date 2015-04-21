using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Moq;
using NUnit.Framework;
using Server.Controllers;
using Server.Logic;

namespace Server.Tests.ControllerTests
{
    [TestFixture]
    public class UserControllerTests
    {
        [Test]
        public void LoginWithAUserThatDoesntExistThrowsException()
        {
            //Assign
            var mock = new Mock<IServerLogic>();
            mock.Setup(t => t.Login(It.IsAny<string>())).Throws(new InvalidOperationException());
            var control = new UsersController(mock.Object);
            
            //Action
            var testDel = new TestDelegate(() => control.Login("doesntexist"));

            //Assert
            Assert.Throws<HttpResponseException>(testDel);
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.ViewModels;
using Common;
using NUnit.Framework;

namespace Client.Tests.ViewModels
{
    [TestFixture]
    class WorkflowViewModelTests
    {
        private WorkflowViewModel _model;
        private WorkflowDto _dto;

        [SetUp]
        public void SetUp()
        {
            _dto = new WorkflowDto {Id = "WorkflowId", Name = "WorkflowName"};
            _model = new WorkflowViewModel(new WorkflowListViewModel(), _dto, new List<string>());
        }

        #region Constructors

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConstructorWithNullArguments()
        {
            //Arrange
            WorkflowListViewModel workflowListViewModel = null;
            WorkflowDto workflowDto = null;
            IList<string> roles = null;

            //Act
            var workflowViewModel = new WorkflowViewModel(workflowListViewModel, workflowDto, roles);

            //Assert
            Assert.IsNull(workflowViewModel);
        }

        [Test]
        public void Test_ConstructorWithLegalArguments()
        {
            //Arrange
            WorkflowListViewModel workflowListViewModel = new WorkflowListViewModel();
            WorkflowDto workflowDto = new WorkflowDto();
            IList<string> roles = new List<string>();

            //Act
            var workflowViewModel = new WorkflowViewModel(workflowListViewModel, workflowDto, roles);

            //Assert
            Assert.IsNotNull(workflowViewModel);

        }
        #endregion

        #region Databindings

        [Test]
        public void Test_SetNamePropertyChanged()
        {
            //Arrange
            var b = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "Name") b = true; };

            //Act
            _model.Name = "Hola";
               
            //Assert
            Assert.IsTrue(b);
        }
        [Test]
        public void Test_SetNamePropertyChangesWorkflowName()
        {
            //Arrange
            

            //Act
            _model.Name = "Hola";

            //Assert
            Assert.AreEqual("Hola", _model.Name);
            Assert.AreEqual("Hola", _dto.Name);
        }
        #endregion
    }
}

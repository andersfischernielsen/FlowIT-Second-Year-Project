using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO.Event;
using Common.DTO.History;
using Event.Controllers;
using Event.Interfaces;
using Event.Logic;
using Event.Models;
using Event.Models.UriClasses;
using Moq;
using NUnit.Framework;

namespace Event.Tests.ControllersTests
{
    [TestFixture]
    class LifeCycleControllerTests
    {
        private IList<HistoryModel> _historyTestList;
        private IList<EventModel> _eventTestList;
        private LifecycleController _toTest;
            
        [TestFixtureSetUp]
        public void SetUp()
        {
            ResetLists();
            var historyMock = new Mock<IEventHistoryLogic>();
            var lifecycleMock = new Mock<ILifecycleLogic>();

            historyMock.Setup(l => l.GetHistoryForEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string wId, string eId) => Task.Run( () => 
                {
                    var models = _historyTestList.Where(x => x.EventId == eId && x.WorkflowId == wId).ToList();
                    var dtos = new List<HistoryDto>();
                    models.ForEach(x => dtos.Add(new HistoryDto(x)));
                    return dtos.AsEnumerable();
                }));

            historyMock.Setup(l => l.SaveException(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((Exception e, string request, string method, string eId, string wId) =>
                    {
                        _historyTestList.Add(new HistoryModel
                        {
                            EventId = eId,
                            WorkflowId = wId,
                            HttpRequestType = request,
                            Message = e.GetType().ToString(),
                            MethodCalledOnSender = method
                        });
                    }
                );

            historyMock.Setup(l => l.SaveSuccesfullCall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string request, string method, string eId, string wId) =>
                    {
                        _historyTestList.Add(new HistoryModel
                        {
                            EventId = eId,
                            WorkflowId = wId,
                            HttpRequestType = request,
                            Message = "Called: " + method,
                            MethodCalledOnSender = method
                        });
                    }
                );

            lifecycleMock.Setup(x => x.CreateEvent(It.IsAny<EventDto>(), It.IsAny<Uri>()))
                .Callback((EventDto dto, Uri uri) =>
                {
                    
                });

            _toTest = new LifecycleController(lifecycleMock.Object, historyMock.Object);
        }

        private EventModel ConvertDtoToEventModel(EventDto dto)
        {
            var conditions = new List<ConditionUri>();
            dto.Conditions.ToList().ForEach(x => conditions.Add(new ConditionUri { UriString = x.Uri.AbsolutePath }));

            var exclusions = new List<ConditionUri>();
            dto.Exclusions.ToList().ForEach(x => conditions.Add(new ConditionUri { UriString = x.Uri.AbsolutePath }));

            //FUCK THIS BULLSHIT.

            return new EventModel
            {
                Executed = dto.Executed,
                WorkflowId = dto.WorkflowId,
                Name = dto.Name,
                Id = dto.EventId,
                Included = dto.Included,
                ConditionUris = conditions,
                // ExclusionUris = exclusions,
                // InclusionUris = dto.Inclusions,
                // Roles = dto.Roles,
                // ResponseUris = dto.Responses,
                Pending = dto.Pending
            };
        }

        [SetUp]
        public void ResetLists()
        {
            _historyTestList = new List<HistoryModel>();
            _eventTestList = new List<EventModel>();
        }

        [Test]
        public void TestCreateEvent()
        {
            
        }
    }
}

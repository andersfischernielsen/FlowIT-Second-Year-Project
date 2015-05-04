using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO.Event;
using Common.DTO.History;
using Common.DTO.Shared;
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
            var historyMock = new Mock<IEventHistoryLogic>(MockBehavior.Strict);
            var lifecycleMock = new Mock<ILifecycleLogic>(MockBehavior.Strict);

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

            lifecycleMock.Setup(m => m.CreateEvent(It.IsAny<EventDto>(), It.IsAny<Uri>()))
                .Callback((EventDto dto, Uri uri) =>
                {
                    _eventTestList.Add(ConvertDtoToEventModel(dto, uri));
                });

            lifecycleMock.Setup(m => m.DeleteEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workflowId, string eventId) =>
                {
                    return Task.Run(() =>
                    {
                        var toRemove = _eventTestList.FirstOrDefault(e => e.WorkflowId == workflowId && e.Id == eventId);
                        if (toRemove != null) _eventTestList.Remove(toRemove);
                    });
                });

            lifecycleMock.Setup(m => m.GetEventDto(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workflowId, string eventId) =>
                {
                    return Task.Run(() => _eventTestList.Where(e => e.Id == eventId && e.WorkflowId == workflowId)
                        .Select(e => ConvertEventModelToDto(e, workflowId, eventId)).First());
                });

            lifecycleMock.Setup(m => m.ResetEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Run(() => true));

            _toTest = new LifecycleController(lifecycleMock.Object, historyMock.Object);
        }

        private static EventModel ConvertDtoToEventModel(EventDto dto, Uri uri)
        {
            var @event = new EventModel
            {
                Id = dto.EventId,
                WorkflowId = dto.WorkflowId,
                Name = dto.Name,
                Roles = dto.Roles.Select(role => new EventRoleModel { WorkflowId = dto.WorkflowId, EventId = dto.EventId, Role = role }).ToList(),
                OwnUri = uri.AbsoluteUri,
                Executed = dto.Executed,
                Included = dto.Included,
                Pending = dto.Pending,
                ConditionUris = dto.Conditions.Select(condition => new ConditionUri { WorkflowId = dto.WorkflowId, EventId = dto.EventId, ForeignEventId = condition.Id, UriString = condition.Uri.AbsoluteUri }).ToList(),
                ResponseUris = dto.Responses.Select(response => new ResponseUri { WorkflowId = dto.WorkflowId, EventId = dto.EventId, ForeignEventId = response.Id, UriString = response.Uri.AbsoluteUri }).ToList(),
                InclusionUris = dto.Inclusions.Select(inclusion => new InclusionUri { WorkflowId = dto.WorkflowId, EventId = dto.EventId, ForeignEventId = inclusion.Id, UriString = inclusion.Uri.AbsoluteUri }).ToList(),
                ExclusionUris = dto.Exclusions.Select(exclusion => new ExclusionUri { WorkflowId = dto.WorkflowId, EventId = dto.EventId, ForeignEventId = exclusion.Id, UriString = exclusion.Uri.AbsoluteUri }).ToList(),
                LockOwner = null,
                InitialExecuted = dto.Executed,
                InitialIncluded = dto.Included,
                InitialPending = dto.Pending
            };

            return @event;
        }

        private static EventDto ConvertEventModelToDto(EventModel e, string workflowId, string eventId)
        {
            var toReturn = new EventDto
            {
                EventId = eventId,
                WorkflowId = workflowId,
                Name = e.Name,
                Roles = e.Roles.Select(role => role.Role),
                Pending = e.Pending,
                Executed = e.Executed,
                Included = e.Included,
                Conditions =
                    e.ConditionUris.Select(
                        uri =>
                            new EventAddressDto
                            {
                                WorkflowId = uri.WorkflowId,
                                Id = uri.EventId,
                                Uri = new Uri(uri.UriString)
                            }),
                Exclusions =
                    e.ExclusionUris.Select(
                        uri =>
                            new EventAddressDto
                            {
                                WorkflowId = uri.WorkflowId,
                                Id = uri.EventId,
                                Uri = new Uri(uri.UriString)
                            }),
                Responses =
                    e.ResponseUris.Select(
                        uri =>
                            new EventAddressDto
                            {
                                WorkflowId = uri.WorkflowId,
                                Id = uri.EventId,
                                Uri = new Uri(uri.UriString)
                            }),
                Inclusions =
                    e.InclusionUris.Select(
                        uri =>
                            new EventAddressDto
                            {
                                WorkflowId = uri.WorkflowId,
                                Id = uri.EventId,
                                Uri = new Uri(uri.UriString)
                            })
            };

            return toReturn;
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

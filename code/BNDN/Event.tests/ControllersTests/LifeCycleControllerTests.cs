using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private Mock<IEventHistoryLogic> _historyMock;
        private Mock<ILifecycleLogic> _lifecycleMock;

        [TestFixtureSetUp]
        public void SetUp()
        {
            ResetLists();
            _historyMock = new Mock<IEventHistoryLogic>(MockBehavior.Strict);
            _lifecycleMock = new Mock<ILifecycleLogic>(MockBehavior.Strict);

            _historyMock.Setup(l => l.GetHistoryForEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string wId, string eId) => Task.Run( () => 
                {
                    var models = _historyTestList.Where(x => x.EventId == eId && x.WorkflowId == wId).ToList();
                    var dtos = new List<HistoryDto>();
                    models.ForEach(x => dtos.Add(new HistoryDto(x)));
                    return dtos.AsEnumerable();
                })).Verifiable();

            _historyMock.Setup(l => l.SaveException(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((Exception e, string request, string method, string eId, string wId) =>
                    {
                       return Task.Run( () => _historyTestList.Add(new HistoryModel
                       {
                           EventId = eId, WorkflowId = wId, HttpRequestType = request, Message = e.GetType().ToString(), MethodCalledOnSender = method
                       }));
                    }
                ).Verifiable();

            _historyMock.Setup(l => l.SaveSuccesfullCall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string request, string method, string eId, string wId) =>
                    {
                        return Task.Run( () => 
                        _historyTestList.Add(new HistoryModel
                        {
                            EventId = eId,
                            WorkflowId = wId,
                            HttpRequestType = request,
                            Message = "Called: " + method,
                            MethodCalledOnSender = method
                        }));
                    }
                ).Verifiable();

            _lifecycleMock.Setup(m => m.CreateEvent(It.IsAny<EventDto>(), It.IsAny<Uri>()))
                .Returns((EventDto dto, Uri uri) => Task.Run(() => _eventTestList.Add(ConvertDtoToEventModel(dto, uri)))).Verifiable();

            _lifecycleMock.Setup(m => m.DeleteEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workflowId, string eventId) =>
                {
                    return Task.Run(() =>
                    {
                        var toRemove = _eventTestList.FirstOrDefault(e => e.WorkflowId == workflowId && e.Id == eventId);
                        if (toRemove != null) _eventTestList.Remove(toRemove);
                    });
                }).Verifiable();

            _lifecycleMock.Setup(m => m.GetEventDto(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workflowId, string eventId) =>
                {
                    return Task.Run(() => _eventTestList.Where(e => e.Id == eventId && e.WorkflowId == workflowId)
                        .Select(e => ConvertEventModelToDto(e, workflowId, eventId)).First());
                });

            _lifecycleMock.Setup(m => m.ResetEvent(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Run(() => true)).Verifiable();

            _toTest = new LifecycleController(_lifecycleMock.Object, _historyMock.Object);
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
        public async void TestCreateEvent()
        {
            //Setup.
            _toTest.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://testing.com/"));

            var @event = new EventModel
            {
                WorkflowId = ")(!&lkjasdkøåøæ+*¨´           $$§§",
                Id = ")(!&lkjasdkøåøæ+*¨´           $$§§",
                Executed = true,
                Included = true,
                Name = ")(!&lkjasdkøåøæ+*¨´           $$§§",
                OwnUri = "http://testing.com/",
                Pending = true, 
            };

            var testRoles = new List<EventRoleModel> { new EventRoleModel { Event = @event, WorkflowId = @")(!&lkjasdkøåøæ+*¨´           $$§§", EventId = ")(!&lkjasdkøåøæ+*¨´           $$§§", Role = ")(!&lkjasdkøåøæ+*¨´           $$§§"} };
            var conditionUris = new List<ConditionUri> { new ConditionUri { Event = @event, EventId = @event.Id, ForeignEventId = "testing", WorkflowId = @event.WorkflowId, UriString = "http://testing.com/" } };
            var exclusionUris = new List<ExclusionUri> { new ExclusionUri { Event = @event, EventId = @event.Id, ForeignEventId = "testing", WorkflowId = @event.WorkflowId, UriString = "http://testing.com/" } };
            var inclusionUris = new List<InclusionUri> { new InclusionUri { Event = @event, EventId = @event.Id, ForeignEventId = "testing", WorkflowId = @event.WorkflowId, UriString = "http://testing.com/" } };
            var responseUris = new List<ResponseUri> { new ResponseUri { Event = @event, EventId = @event.Id, ForeignEventId = "testing", WorkflowId = @event.WorkflowId, UriString = "http://testing.com/" } };

            @event.Roles = testRoles;
            @event.ConditionUris = conditionUris;
            @event.ExclusionUris = exclusionUris;
            @event.InclusionUris = inclusionUris;
            @event.ResponseUris = responseUris;

            //Execute.
            await _toTest.CreateEvent(ConvertEventModelToDto(@event, ")(!&lkjasdkøåøæ+*¨´           $$§§", ")(!&lkjasdkøåøæ+*¨´           $$§§"));

            //Assert.
            _lifecycleMock.Verify(m => m.CreateEvent(It.IsAny<EventDto>(), It.IsAny<Uri>()), Times.Once);
            Assert.IsTrue(_eventTestList.Any()); //The list now has an EventModel.

            var eventInList = _eventTestList.First();
            Assert.AreEqual(@event.WorkflowId, eventInList.WorkflowId);
            Assert.AreEqual(@event.ConditionUris.First().EventId, eventInList.ConditionUris.First().EventId);
            Assert.AreEqual(@event.ExclusionUris.First().EventId, eventInList.ExclusionUris.First().EventId);
            Assert.AreEqual(@event.Id, eventInList.Id);
            Assert.AreEqual(@event.Included, eventInList.Included);
            Assert.AreEqual(@event.InclusionUris.First().EventId, eventInList.InclusionUris.First().EventId);
            Assert.AreEqual(@event.ResponseUris.First().EventId, eventInList.ResponseUris.First().EventId);
            Assert.AreEqual(@event.Roles.First().Role, eventInList.Roles.First().Role);
            Assert.AreEqual(@event.Executed, eventInList.Executed);
            Assert.AreEqual(@event.Name, eventInList.Name);
            Assert.AreEqual(@event.OwnUri, eventInList.OwnUri);
            Assert.AreEqual(@event.Pending, eventInList.Pending);
        }
    }
}

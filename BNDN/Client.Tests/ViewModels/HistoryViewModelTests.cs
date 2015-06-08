﻿using System;
using Client.ViewModels;
using Common.DTO.History;
using NUnit.Framework;

namespace Client.Tests.ViewModels
{
    [TestFixture]
    class HistoryViewModelTests
    {
        private HistoryViewModel _model;
        private HistoryDto _dto;

        [SetUp]
        public void SetUp()
        {
            _dto = new HistoryDto();

            _model = new HistoryViewModel(_dto);
        }

        #region Constructors

        [Test]
        public void Constructor_NoParameter()
        {
            // Act
            var model = new HistoryViewModel();

            // Assert
            Assert.IsNotNull(model);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullParameter()
        {
            // Act
            var model = new HistoryViewModel(null);

            // Assert
            Assert.Fail(); // Should never run.
        }

        [Test]
        public void Constructor_Parameter()
        {
            // Act
            var model = new HistoryViewModel(new HistoryDto());

            // Assert
            Assert.IsNotNull(model);
        }
        #endregion

        #region Databindings
        [TestCase(""),
         TestCase(null),
         TestCase("Rubbish"),
         TestCase("Very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, long WorkflowId")]
        public void WorkflowId_PropertyChanged(string workflowId)
        {
            // Arrange
            var changed = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "WorkflowId") changed = true; };

            // Act
            _model.WorkflowId = workflowId;

            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(workflowId, _model.WorkflowId);
        }

        [TestCase(""),
         TestCase(null),
         TestCase("Rubbish"),
         TestCase("Very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, long EventId")]
        public void EventId_PropertyChanged(string eventId)
        {
            // Arrange
            var changed = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "EventId") changed = true; };

            // Act
            _model.EventId = eventId;

            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(eventId, _model.EventId);
        }

        [TestCase(""),
         TestCase(null),
         TestCase("Rubbish"),
         TestCase("Very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, long Message")]
        public void Message_PropertyChanged(string message)
        {
            // Arrange
            var changed = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "Message") changed = true; };

            // Act
            _model.Message = message;

            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(message, _model.Message);
        }

        [TestCase(""),
         TestCase(null),
         TestCase("Rubbish"),
         TestCase("Very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, very, long Title")]
        public void Title_PropertyChanged(string title)
        {
            // Arrange
            var changed = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "Title") changed = true; };

            // Act
            _model.Title = title;

            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(title, _model.Title);
        }

        [Test]
        public void TimeStamp_PropertyChanged()
        {
            // Arrange
            var changed = false;
            _model.PropertyChanged += (o, s) => { if (s.PropertyName == "TimeStamp") changed = true; };
            var dt = DateTime.Now;

            // Truncate milliseconds.
            dt = dt.AddTicks(-(dt.Ticks % TimeSpan.TicksPerSecond));

            // Act
            _model.TimeStamp = dt;

            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(dt, _model.TimeStamp);
        }
        #endregion
    }
}

using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Client.Connections;
using Client.Exceptions;
using Common;
using Common.Exceptions;

namespace Client.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly EventAddressDto _eventAddressDto;
        private EventStateDto _eventStateDto;
        private readonly WorkflowViewModel _parent;

        public EventViewModel(EventAddressDto eventAddressDto, WorkflowViewModel workflow)
        {
            _eventAddressDto = eventAddressDto;
            _parent = workflow;
            _eventStateDto = new EventStateDto();
            GetState();
        }

        #region Databindings

        public string Id
        {
            get { return _eventAddressDto.Id; }
            set
            {
                _eventAddressDto.Id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string Name
        {
            get
            {
                return _eventStateDto.Name;
            }
            set
            {
                _eventStateDto.Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public Uri Uri
        {
            get { return _eventAddressDto.Uri; }
            set
            {
                _eventAddressDto.Uri = value;
                NotifyPropertyChanged("Uri");
            }
        }

        public bool Pending
        {
            get { return _eventStateDto.Pending; }
            set
            {
                _eventStateDto.Pending = value;
                NotifyPropertyChanged("Pending");
                NotifyPropertyChanged("PendingColor");
            }
        }

        public Brush PendingColor
        {
            get
            {
                if (Pending)
                {
                    var path = Path.Combine(Environment.CurrentDirectory, "Assets", "Pending.png");
                    var uri = new Uri(path);
                    return new ImageBrush(new BitmapImage(uri));
                }
                return null;
            }
        }

        public bool Executed
        {
            get { return _eventStateDto.Executed; }
            set
            {
                _eventStateDto.Executed = value;
                NotifyPropertyChanged("Executed");
                NotifyPropertyChanged("ExecutedColor");
            }
        }

        public Brush ExecutedColor
        {
            get
            {
                if (Executed)
                {
                    var path = Path.Combine(Environment.CurrentDirectory, "Assets", "Executed.png");
                    var uri = new Uri(path);
                    return new ImageBrush(new BitmapImage(uri));
                }
                return null;
            }
        }
        public bool Included
        {
            get { return _eventStateDto.Included; }
            set
            {
                _eventStateDto.Included = value;
                NotifyPropertyChanged("Included");
                NotifyPropertyChanged("IncludedColor");
            }
        }

        public Brush IncludedColor
        {
            get { return Included ? new SolidColorBrush(Colors.DeepSkyBlue) : null; }
        }

        public bool Executable
        {
            get { return _eventStateDto.Executable; }
            set
            {
                _eventStateDto.Executable = value;
                NotifyPropertyChanged("Executable");
            }
        }

        public string Status
        {
            get { return _parent.Status; }
            set { _parent.Status = value; }
        }

        #endregion

        #region Actions

        public async void GetState()
        {
            Status = "";
            try
            {
                using (IEventConnection eventConnection = new EventConnection(_eventAddressDto.Uri))
                {
                    _eventStateDto = await eventConnection.GetState(_parent.WorkflowId, _eventAddressDto.Id);
                }
                NotifyPropertyChanged("");
            }
            catch (NotFoundException)
            {
                Status = "The event could not be found. Please refresh the workflow";
            }
            catch (LockedException)
            {
                Status = "The event is currently locked. Please try again later.";
            }
            catch (HostNotFoundException)
            {
                Status =
                    "The host of the event was not found. Please refresh the workflow. If the problem persists, contact you Flow administrator";
            }
            catch (Exception e)
            {
                Status = e.Message;
            }
        }

        /// <summary>
        /// This method gets called by the Execute Button in the UI
        /// </summary>
        public async void Execute()
        {
            Status = "";
            try
            {
                using (IEventConnection eventConnection = new EventConnection(_eventAddressDto.Uri))
                {
                    await eventConnection.Execute(_parent.WorkflowId, _eventAddressDto.Id);
                }
                _parent.GetEvents();
            }
            catch (NotFoundException)
            {
                Status = "The event could not be found. Please refresh the workflow";
            }
            catch (UnauthorizedException)
            {
                Status = "You do not have the rights to execute this event";
            }
            catch (LockedException)
            {
                Status = "The event is currently locked. Please try again later.";
            }
            catch (NotExecutableException)
            {
                Status = "The event is currently not executable. Please refresh the workflow";
            }
            catch (HostNotFoundException)
            {
                Status =
                    "The host of the event was not found. Please refresh the workflow. If the problem persists, contact you Flow administrator";
            }
            catch (Exception e)
            {
                Status = e.Message;
            }

        }
        #endregion
    }
}

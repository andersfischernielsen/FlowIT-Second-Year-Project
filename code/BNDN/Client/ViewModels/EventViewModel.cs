using System;
using System.Windows;
using Common;

namespace Client.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly EventAddressDto _eventAddressDto;
        private EventStateDto _eventStateDto;
        private WorkflowViewModel _parent;

        public EventViewModel()
        {
            _eventAddressDto = new EventAddressDto();
            _eventStateDto = new EventStateDto(){Executable = true};
            GetState();
        }
        public EventViewModel(EventAddressDto eventAddressDto, WorkflowViewModel workflow)
        {
            _eventAddressDto = eventAddressDto;
            _parent = workflow;
            _eventStateDto = new EventStateDto() { Executable = true };
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
            }
        }
        public bool Executed
        {
            get { return _eventStateDto.Executed; }
            set
            {
                _eventStateDto.Executed = value;
                NotifyPropertyChanged("Executed");
            }
        }
        public bool Included
        {
            get { return _eventStateDto.Included; }
            set
            {
                _eventStateDto.Included = value;
                NotifyPropertyChanged("Included");
            }
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

        #endregion

        #region Actions

        public async void GetState()
        {
            var eventConnection = new EventConnection(_eventAddressDto);
            try
            {
                _eventStateDto = await eventConnection.GetState();
                NotifyPropertyChanged("");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.GetType());
            }
        }

        public async void Execute()
        {
            try
            {
                var eventConnection = new EventConnection(_eventAddressDto);
                await eventConnection.Execute(true, _parent.WorkflowId);
                _parent.GetEvents();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.GetType());
            }

        }
        #endregion

        public override string ToString()
        {
            return string.Format("Id: {0} - URI: {1}", Id, Uri.ToString());
        }
    }
}

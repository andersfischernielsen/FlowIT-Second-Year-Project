using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly EventAddressDto _eventAddressDto;
        private EventStateDto _eventStateDto;
        public EventViewModel()
        {
            _eventAddressDto = new EventAddressDto();
            _eventStateDto = new EventStateDto(){Executable = true};
            GetState();
        }
        public EventViewModel(EventAddressDto eventAddressDto)
        {
            _eventAddressDto = eventAddressDto;
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
            _eventStateDto = await eventConnection.GetState();
            NotifyPropertyChanged("");
        }

        public async void Execute()
        {
            var eventConnection = new EventConnection(_eventAddressDto);
            await eventConnection.Execute(true);
        }
        #endregion

        public override string ToString()
        {
            return "Id: " + Id + " - URI: " + Uri.ToString();
        }
    }
}

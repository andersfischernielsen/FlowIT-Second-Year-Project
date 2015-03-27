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
        private EventAddressDto __eventAddressDto;
        private EventStateDto _eventStateDto;
        public EventViewModel()
        {
            __eventAddressDto = new EventAddressDto();
            _eventStateDto = new EventStateDto(){Executable = true};
            GetState();
        }
        public EventViewModel(EventAddressDto _eventAddressDto)
        {
            __eventAddressDto = _eventAddressDto;
            _eventStateDto = new EventStateDto() { Executable = true };
            GetState();
        }

        #region Databindings

        public string Id
        {
            get { return __eventAddressDto.Id; }
            set
            {
                __eventAddressDto.Id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public Uri Uri
        {
            get { return __eventAddressDto.Uri; }
            set
            {
                __eventAddressDto.Uri = value;
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

        public void GetState()
        {
            Task.Run(async () =>
            {
                try
                {
                    var eventConnection = new EventConnection(__eventAddressDto);
                    _eventStateDto = await eventConnection.GetState();
                    NotifyPropertyChanged("");
                }
                catch (Exception)
                {

                    throw;
                }
            });
        }
        public void Execute()
        {
            Task.Run(async () =>
            {
                try
                {
                    var eventConnection = new EventConnection(__eventAddressDto);
                    await eventConnection.Execute(true);
                }
                catch (Exception)
                {
                    
                    throw;
                }
            });
        }
        #endregion

        public override string ToString()
        {
            return "Id: " + Id + " - URI: " + Uri.ToString();
        }
    }
}

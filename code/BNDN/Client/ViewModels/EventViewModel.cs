﻿using System;
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
        public EventViewModel()
        {
            __eventAddressDto = new EventAddressDto();
        }
        public EventViewModel(EventAddressDto _eventAddressDto)
        {
            __eventAddressDto = _eventAddressDto;
        }

        #region Databindings

        public int Id
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

        #endregion

        #region Actions

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

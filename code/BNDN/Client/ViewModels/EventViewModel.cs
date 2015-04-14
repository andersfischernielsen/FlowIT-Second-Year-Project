using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Client.Connections;
using Common;

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
            GetState(); // Dont wait this! 
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

        #endregion

        #region Actions

        public async Task GetState()
        {
            IEventConnection eventConnection = new EventConnection(_eventAddressDto, _parent.WorkflowId);
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

        /// <summary>
        /// This method gets called by the Execute Button in the UI
        /// </summary>
        public async void Execute()
        {
            try
            {
                IEventConnection eventConnection = new EventConnection(_eventAddressDto, _parent.WorkflowId);
                await eventConnection.Execute(true);
                _parent.GetEvents();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.GetType());
            }

        }
        #endregion
    }
}

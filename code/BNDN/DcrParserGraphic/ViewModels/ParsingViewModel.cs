using System;
using System.Linq;
using System.Windows;
using DCRParserGraphic;
using Microsoft.Win32;

namespace DcrParserGraphic.ViewModels
{
    public class ParsingViewModel : ViewModelBase
    {
        private string _xmlFilePath;
        private string _workflowId;
        private string _workflowName;
        private string _serverUri;
        private string _eventUris;

        public string XmlFilePath
        {
            get { return _xmlFilePath; }
            set
            {
                _xmlFilePath = value;
                NotifyPropertyChanged("XmlFilePath");
            }
        }

        public string WorkflowId
        {
            get { return _workflowId; }
            set
            {
                _workflowId = value;
                NotifyPropertyChanged("WorkflowId");
            }
        }

        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                NotifyPropertyChanged("WorkflowName");
            }
        }

        public string ServerUri
        {
            get { return _serverUri; }
            set
            {
                _serverUri = value;
                NotifyPropertyChanged("ServerUri");
            }
        }

        public string EventUris
        {
            get { return _eventUris; }
            set
            {
                _eventUris = value;
                NotifyPropertyChanged("EventUris");
            }
        }

        public void Choose()
        {
            var openFileDialog1 = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", FilterIndex = 1 };

            // Set filter options and filter index.
            if (openFileDialog1.ShowDialog() ?? false)
            {
                var file = openFileDialog1.FileName;
                XmlFilePath = file;
            }
        }

        public async void Convert()
        {
            if (!string.IsNullOrEmpty(XmlFilePath) && !string.IsNullOrEmpty(EventUris) && !string.IsNullOrEmpty(WorkflowId))
            {
                try
                {
                    var eventUrls = GetUrls(EventUris);
                    await DcrParser.Parse(XmlFilePath, WorkflowId, eventUrls).CreateJsonFile();

                    MessageBox.Show("Everything went OK. The file should have been created in the same place as this exe file");
                    ClearFields();
                }
                catch (Exception)
                {
                    MessageBox.Show("Something went wrong, probably bad file or file doesnt exist");
                }
            }
        }

        public async void Upload()
        {
            if (!string.IsNullOrEmpty(XmlFilePath) && !string.IsNullOrEmpty(EventUris) && !string.IsNullOrEmpty(WorkflowId))
            {
                try
                {
                    var eventUrls = GetUrls(EventUris);

                    //var ips = TextBoxUrl.Text.Replace(" ","").Split(',');
                    var parser = DcrParser.Parse(XmlFilePath, WorkflowId, eventUrls);
                    var map = parser.GetMap();
                    var roles = parser.GetRoles();
                    var uploader = new EventUploader(WorkflowId, ServerUri, parser.IdToAddress);
                    await uploader.CreateWorkflow(WorkflowName);
                    await uploader.Upload(map.Values.ToList());
                    await uploader.UploadUsers(roles);
                    MessageBox.Show("Everything went OK. The file should have been uploaded to the given urls");
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Something went wrong, probably bad file or file doesnt exist" + Environment.NewLine + ex);
                }
            }
        }

        private void ClearFields()
        {
            XmlFilePath = "";
            EventUris = "";
            ServerUri = "";
            WorkflowId = "";
            WorkflowName = "";
        }

        private static string[] GetUrls(string eventUrls)
        {
            return eventUrls.Replace(" ", "").Split(',');
        }
    }
}

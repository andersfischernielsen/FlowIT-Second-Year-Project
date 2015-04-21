﻿using System;
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
        private bool _createWorkflow;
        private bool _createUsers;
        private string _defaultPassword;

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

        public string DefaultPassword
        {
            get { return _defaultPassword; }
            set
            {
                _defaultPassword = value;
                NotifyPropertyChanged("DefaultPassword");
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

        public bool CreateWorkflow
        {
            get { return _createWorkflow; }
            set
            {
                _createWorkflow = value;
                NotifyPropertyChanged("CreateWorkflow");
            }
        }

        public bool CreateUsers
        {
            get { return _createUsers; }
            set
            {
                _createUsers = value;
                NotifyPropertyChanged("CreateUsers");
            }
        }

        public void Choose()
        {
            var openFileDialog1 = new OpenFileDialog { Filter = (string) Application.Current.FindResource("XmlFileType"), FilterIndex = 1 };

            // Set filter options and filter index.
            if (!(openFileDialog1.ShowDialog() ?? false)) return;

            var file = openFileDialog1.FileName;
            XmlFilePath = file;
        }

        public async void Convert()
        {
            if (string.IsNullOrEmpty(XmlFilePath) || string.IsNullOrEmpty(EventUris) || string.IsNullOrEmpty(WorkflowId))
            {
                // Todo: Shouldn't this show some kind of error message to the user?
                await DcrParser.Parse(XmlFilePath, WorkflowId, new string[1]).CreateJsonFile();
            }
            else
            {
                try
                {
                    var eventUrls = GetUrls(EventUris);
                    await DcrParser.Parse(XmlFilePath, WorkflowId, eventUrls).CreateJsonFile();

                    MessageBox.Show((string)Application.Current.FindResource("ParsingToJsonOk"));
                    ClearFields();
                }
                catch (Exception)
                {
                    MessageBox.Show((string)Application.Current.FindResource("ParsingToJsonProblem"));
                }
            }
        }

        public async void Upload()
        {
            if (!string.IsNullOrEmpty(XmlFilePath) && !string.IsNullOrEmpty(EventUris) && !string.IsNullOrEmpty(WorkflowId))
            {
                DcrParser parser;
                try
                {
                    var eventUrls = GetUrls(EventUris);

                    //var ips = TextBoxUrl.Text.Replace(" ","").Split(',');
                    parser = DcrParser.Parse(XmlFilePath, WorkflowId, eventUrls);
                }
                catch (Exception ex)
                {
                    MessageBox.Show((string)Application.Current.FindResource("ParsingToJsonOk") + Environment.NewLine + ex);
                    return;
                }
                var map = parser.GetMap();
                var roles = parser.GetRoles();
                var uploader = new EventUploader(WorkflowId, ServerUri, parser.IdToAddress);
                if (CreateWorkflow)
                {
                    try
                    {
                        await uploader.CreateWorkflow(WorkflowName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(Application.Current.FindResource("UploadWorkflowFailed") +
                                        Environment.NewLine + e);
                        return;
                    }
                }
                try
                {
                    await uploader.Upload(map.Values.ToList());
                }
                catch (Exception e)
                {
                    MessageBox.Show(Application.Current.FindResource("UploadEventsFailed") + Environment.NewLine + e);
                    return;
                }
                if (CreateUsers)
                {
                    try
                    {
                        await uploader.UploadUsers(roles, DefaultPassword);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(Application.Current.FindResource("UploadUsersFailed") + Environment.NewLine +
                                        e);
                        return;
                    }
                }
                MessageBox.Show((string)Application.Current.FindResource("UploadOk"));
                ClearFields();
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

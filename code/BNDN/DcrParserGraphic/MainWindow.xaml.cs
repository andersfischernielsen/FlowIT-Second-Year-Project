using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCRParserGraphic;
using Microsoft.Win32;

namespace DcrParserGraphic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonChoose_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog1.FilterIndex = 1;
            if ((bool)openFileDialog1.ShowDialog()) //AV AV AV
            {
                var file = openFileDialog1.FileName;
                TextBoxFile.Text = file;
            }
        }

        private void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxFile.Text) && !string.IsNullOrEmpty(TextBoxUrl.Text) && !string.IsNullOrEmpty(TextBoxWorkflowName.Text))
            {
                try
                {
                    //var ips = TextBoxUrl.Text.Split(',');
                    new DcrParser(TextBoxFile.Text, TextBoxWorkflowName.Text, TextBoxUrl.Text).CreateXmlFile();

                    MessageBox.Show("Everything went OK. The file should have been created in the same place as this exe file");
                    TextBoxFile.Text = "";
                    TextBoxUrl.Text = "";
                    TextBoxWorkflowName.Text = "";
                }
                catch (Exception)
                {
                    MessageBox.Show("Something went wrong, probably bad file or file doesnt exist");
                }
            }
        }

        private void hiddenbutton_onclick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.staggeringbeauty.com/");
        }

        private async void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxFile.Text) && !string.IsNullOrEmpty(TextBoxUrl.Text) && !string.IsNullOrEmpty(TextBoxWorkflowName.Text))
            {
                try
                {
                    //var ips = TextBoxUrl.Text.Replace(" ","").Split(',');
                    var parser = new DcrParser(TextBoxFile.Text, TextBoxWorkflowName.Text, TextBoxUrl.Text);
                    var map = parser.GetMap();
                    var roles = parser.GetRoles();
                    var uploader = new EventUploader(TextBoxWorkflowName.Text);
                    await uploader.Upload(map.Values.ToList());
                    await uploader.UploadUsers(roles);
                    MessageBox.Show("Everything went OK. The file should have been uploaded to the given urls");
                    TextBoxFile.Text = "";
                    TextBoxUrl.Text = "";
                    TextBoxWorkflowName.Text = "";
                }
                catch (Exception)
                {
                    MessageBox.Show("Something went wrong, probably bad file or file doesnt exist");
                }
            }
        }
    }
}

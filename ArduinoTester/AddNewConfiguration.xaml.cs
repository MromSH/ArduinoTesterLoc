using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для AddNewConfiguration.xaml
    /// </summary>
    public partial class AddNewConfiguration : Window
    {

        AppContext database;

        private string FileContent;
        private byte[] SchemePicture; 

        private Page1 page { get; set; }
        public AddNewConfiguration(Page1 page1)
        {
            InitializeComponent();

            database = new AppContext();

            this.page = page1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string naming = NamingTextBox.Text.Trim();
            string description = DescriptionTextBox.Text;
            string iNOFile = FileContent;
            byte[] scheme = SchemePicture;

            if (naming == null || naming == "")
            {
                MessageBox.Show("Give a name for your configuration", "Empty field", MessageBoxButton.OK);
            }
            else if (FileContent == null)
            {
                MessageBox.Show("Select a file for your programm", "Empty field", MessageBoxButton.OK);
            }
            else
            {
                bool fl = false;
                database = new AppContext();

                ObservableCollection<Configuration> configs = new ObservableCollection<Configuration>(database.Configurations);

                foreach(Configuration config in configs)
                {
                    if (naming == config.naming)
                    {
                        fl = true;
                        MessageBox.Show("Such name is already existing. Choose another one", "Invalid name of configuration", MessageBoxButton.OK);
                        break;
                    }
                }
                if (!fl)
                {
                    Configuration configuration = new Configuration(naming, description, iNOFile, scheme);
                    database.Configurations.Add(configuration);
                    database.SaveChanges();

                    page.Refresh(configuration);
                    this.Close();
                }
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Arduino Files (*.c, *.cpp)|*.c;*.cpp";
            if (ofd.ShowDialog() == true)
            {
                string selectedfile = ofd.FileName;
                if (File.Exists(selectedfile))
                {
                    FileNameTextBlock.Text = System.IO.Path.GetFileName(selectedfile);
                    FileContent = File.ReadAllText(selectedfile);
                }
                
            }
        }

        private void SchemeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pictures (*.png, *.jpg)|*.png;*.jpg";
            if (ofd.ShowDialog() == true)
            {
                string selectedfile = ofd.FileName;
                if (File.Exists(selectedfile))
                {
                    SchemeTextBlock.Text = System.IO.Path.GetFileName(selectedfile);
                    SchemePicture = File.ReadAllBytes(selectedfile);
                    SchemeImage.Source = new BitmapImage(new Uri(selectedfile));
                }
            }
        }
    }
}

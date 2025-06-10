using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
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

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly AppContext database;
        private short id;

        private string Filecontent;
        private byte[] SchemeImg;

        private Page1 page;

        public short Id
        {
            get { return id; }
            set { id = value; }
        }

        public EditWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EditWindow(short id, Page1 page) : this()
        {
            this.Id = id;
            database = new AppContext();

            ObservableCollection<Configuration> configs = new ObservableCollection<Configuration>(database.Configurations);
            foreach (Configuration cfg in configs)
            {
                if (cfg.id == Id)
                {
                    DescriptionTextBox.Text = cfg.description;
                    NamingTextBox.Text = cfg.naming;
                    if (cfg.scheme == null)
                    {
                        SchemeImage.Source = new BitmapImage(new Uri("pack://application:,,,/ArduinoTester;component/static_resources/emptyPic.jpg"));
                    }
                    else
                    {
                        using (var memoryStream = new MemoryStream(cfg.scheme))
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();

                            SchemeImage.Source = bitmapImage;
                        }
                    }
                }
            }

            this.page = page;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string naming = NamingTextBox.Text.Trim();
            string description = DescriptionTextBox.Text;
            string iNOFile = Filecontent;
            byte[] scheme = SchemeImg;

            if (naming == null || naming == "")
            {
                MessageBox.Show("Naming field is empty", "Empty field", MessageBoxButton.OK);
            }
            else
            { 
                ObservableCollection<Configuration> configs = new ObservableCollection<Configuration>(database.Configurations);
                foreach (Configuration cfg in configs)
                {
                    if (cfg.id == Id)
                    {
                        bool fl = false;
                        if (naming != null && naming.Trim(' ') != "")
                        {
                            foreach (Configuration config in configs)
                            {
                                if (naming == config.naming)
                                {
                                    fl = true;
                                    MessageBox.Show("Such name is already existing. Choose another one", "Invalid name of configuration", MessageBoxButton.OK);
                                    break;
                                }
                            }
                        }
                        if (!fl)
                        {
                            if (naming != null && naming.Trim(' ') != "")
                            {
                                cfg.naming = naming;
                            }
                            cfg.description = description;
                            if (Filecontent != null)
                            {
                                cfg.inofile = iNOFile;
                            }
                            if (SchemeImg != null)
                            {
                                cfg.scheme = scheme;
                            }

                            database.SaveChanges();
                            page.Remove_cfg(Id);
                            page.Refresh(cfg);
                        }
                    }
                }

                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Arduino Files (*.c, *.cpp)|*.c;*.cpp";
            if (ofd.ShowDialog() == true)
            {
                string selectedfile = ofd.FileName;
                if (File.Exists(selectedfile))
                {
                    FileNameTextBlock.Text = System.IO.Path.GetFileName(selectedfile);
                    Filecontent = File.ReadAllText(selectedfile);
                }

            }

        }

        private void ChangeSchemeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pictures (*.png, *.jpg)|*.png;*.jpg";
            if (ofd.ShowDialog() == true)
            {
                string selectedfile = ofd.FileName;
                if (File.Exists(selectedfile))
                {
                    SchemeTextBlock.Text = System.IO.Path.GetFileName(selectedfile);
                    SchemeImg = File.ReadAllBytes(selectedfile);
                    SchemeImage.Source = new BitmapImage(new Uri(selectedfile));
                }
            }
        }
    }
}

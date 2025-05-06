using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly AppContext database;
        private short id;

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

        public EditWindow(short id): this()
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
                }
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeSchemeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

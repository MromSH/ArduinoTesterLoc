using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private readonly AppContext database;

        public Page1()
        {
            InitializeComponent();
            database = new AppContext();

            ObservableCollection <Configuration> configs = new ObservableCollection<Configuration>(database.Configurations);

            foreach (Configuration config in configs)
            {
                lstConfigs.Items.Add(config);
            }
            this.DataContext = configs;
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewConfiguration addNewConfiguration = new AddNewConfiguration(this);
            addNewConfiguration.ShowDialog();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Configuration configuration)
            {
                Testing testing = new Testing(configuration.naming, configuration.id);
                testing.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Configuration configuration)
            {
                lstConfigs.Items.Remove(configuration);
                database.Configurations.Remove(configuration);
                database.SaveChanges();
            }
        }

        public void Refresh(Configuration configuration)
        {
            if (configuration != null)
            {
                lstConfigs.Items.Add(configuration);
                configuration = null;
            }
        }

        public void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Configuration configuration)
            {
                EditWindow editWindow = new EditWindow(configuration.id);
                editWindow.ShowDialog();
            }
        }
    }
}

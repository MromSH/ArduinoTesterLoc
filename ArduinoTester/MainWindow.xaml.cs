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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<Page> pages;
        protected int index;
        public MainWindow()
        {
            InitializeComponent();

            this.pages = new List<Page>();
            index = 0;

            pages.Add(new Page1());
            pages.Add(new Page2());

            Mainframe.Content = pages[index];
        }

        public void ManualButton_Click(object sender, RoutedEventArgs e) 
        {
            if (index == 1)
            {
                index = 0;
                Mainframe.Content = pages[index];
            }
        }

        public void ConfigurationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                index = 1;
                Mainframe.Content = pages[index];
            }

        }
    }
}

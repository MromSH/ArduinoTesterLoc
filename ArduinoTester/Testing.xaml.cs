using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.IO;
using System.Diagnostics;
using ArduinoTester.Properties;
using System.IO.Ports;
using System.Reflection;

namespace ArduinoTester
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Testing : Window
    {
        private readonly AppContext database;
        private short id;

        private string file;
        
        private bool fl = false;

        private SerialPort serialPort;

        private Dictionary<string, string> plate_types = new Dictionary<string, string>()
            {
                {"Uno", "uno"},
                {"Nano", "nano" },
                {"Nano (CH340)", "nano:cpu=atmega328old" },
                {"Mega 2560", "mega:cpu=atmega2560" },
                {"Leonardo", "leonardo" },
            };

        public short Id
        {
            get { return id; }
            set { id = value; }
        }

        public string File
        {
            get { return file; }
            set { file = value; }
        }
        public Testing()
        {
            InitializeComponent();
            DataContext = this;


        }

        public Testing(string name, short id) : this()
        {
            NamingTextBlock.Text = name;
            this.Id = id;

            database = new AppContext();

            ObservableCollection<Configuration> configs = new ObservableCollection<Configuration>(database.Configurations);
            foreach (Configuration cfg in configs)
            {
                if (cfg.id == Id)
                {
                    DescriptiomTextBlock.Text = cfg.description;
                    this.File = cfg.inofile;
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

            HashSet<string> av_ports = new HashSet<string>(SerialPort.GetPortNames());

            foreach (string portname in av_ports)
            {
                Ports.Items.Add(portname);
            }

            foreach (string plate_type in plate_types.Keys)
            {
                PlateType.Items.Add(plate_type);
            }


        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string selected_port = Ports.Text;
            if (string.IsNullOrEmpty(Ports.Text) || string.IsNullOrEmpty(PlateType.Text))
            {
                MessageBox.Show("Select type of Arduino plate and COM-port", "Error", MessageBoxButton.OK);
            }
            else
            {
                string selected_plate_type = plate_types[PlateType.Text];

                string exePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = System.IO.Path.Combine(exePath, "CurrentProjRun", "CurrentProjRun.ino");

                string inputDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CurrentProjRun");
                string arduinoCLIFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sources", "arduino-cli_1.2.2_Windows_32bit", "arduino-cli.exe");

                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    sw.Write(File);
                }

                ProcessStartInfo processStartInfo20 = new ProcessStartInfo();
                processStartInfo20.FileName = arduinoCLIFullPath;
                processStartInfo20.Arguments = " compile --fqbn arduino:avr:" + selected_plate_type + " .";
                processStartInfo20.WorkingDirectory = inputDir;
                processStartInfo20.RedirectStandardOutput = true;
                processStartInfo20.RedirectStandardError = true;
                processStartInfo20.UseShellExecute = false;
                processStartInfo20.CreateNoWindow = true;
                processStartInfo20.StandardErrorEncoding = Encoding.Default;

                Process compilling = Process.Start(processStartInfo20);
                string errors1 = compilling.StandardError.ReadToEnd();

                string[] ers1 = errors1.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string errorS1 = string.Join(Environment.NewLine, ers1.Where(line => line.IndexOf("error:", StringComparison.OrdinalIgnoreCase) >= 0));

                if (!string.IsNullOrEmpty(errorS1))
                {
                    ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] (Error occured during compilation) {errorS1}");
                    ConsoleLine.ScrollToEnd();
                } 
                else
                {
                    ProcessStartInfo processStartInfo21 = new ProcessStartInfo();
                    processStartInfo21.FileName = arduinoCLIFullPath;
                    processStartInfo21.Arguments = " upload -p " + selected_port + " --fqbn arduino:avr:" + selected_plate_type + " CurrentProjRun.ino";
                    processStartInfo21.WorkingDirectory = inputDir;
                    processStartInfo21.RedirectStandardOutput = true;
                    processStartInfo21.RedirectStandardError = true;
                    processStartInfo21.UseShellExecute = false;
                    processStartInfo21.CreateNoWindow = true;
                    processStartInfo21.StandardErrorEncoding = Encoding.Default;

                    Process flashing = Process.Start(processStartInfo21);
                    string errors2 = flashing.StandardError.ReadToEnd();

                    string[] ers2 = errors2.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

                    string avrdudeEr = string.Join(Environment.NewLine, ers2.Where(line => line.IndexOf("avrdude:", StringComparison.OrdinalIgnoreCase) >= 0));
                    string otherEr2 = string.Join(Environment.NewLine, ers2.Where(line => line.IndexOf("error:", StringComparison.OrdinalIgnoreCase) >= 0));
                    if (!string.IsNullOrEmpty(avrdudeEr))
                    {
                        ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] (Error occured during flashing) {avrdudeEr}");
                        ConsoleLine.ScrollToEnd();
                    }
                    if (!string.IsNullOrEmpty(otherEr2))
                    {
                        ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] (Error occured during flashing) {otherEr2}");
                        ConsoleLine.ScrollToEnd();
                    }
                    if (File.Contains("Serial.begin("))
                    {
                        int pos = File.IndexOf("Serial.begin(");
                        pos += 13;
                        string spd = "";
                        while (Char.IsDigit(File[pos]))
                        {
                            spd += File[pos];
                            pos++;
                        }
                        InitializeSerial(selected_port, int.Parse(spd));
                    }
                }
            }
        }
        private void InitializeSerial(string selected_port, int speed)
        {
            serialPort = new SerialPort(selected_port, speed);
            serialPort.DataReceived += SerialDataR;

            try
            {
                serialPort.Open();
                ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] Serial was opened\n");
                fl = true;
            }
            catch(Exception ex) 
            {
                ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] Error of opening Serial: {ex.Message}\n");
            }
        }

        private void SerialDataR(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting().Trim();
    
            if (!string.IsNullOrEmpty(data))
            {
                Dispatcher.Invoke(() =>
                    {
                        ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] {data}\n");
                        ConsoleLine.ScrollToEnd();
                    }
                );
            }
        }

        private void SendToCOMButton_Click(object sender, RoutedEventArgs e)
        {
            if (fl && !string.IsNullOrWhiteSpace(InputString.Text))
            {
                try
                {
                    serialPort.WriteLine(InputString.Text);
                    ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] {InputString.Text}\n");
                    InputString.Clear();
                }
                catch (Exception ex)
                {
                    ConsoleLine.AppendText($"[{DateTime.Now:HH:mm:ss}] Error of sending: {ex.Message}\n");
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            base.OnClosed(e);
            fl = false;
        }
    }
}

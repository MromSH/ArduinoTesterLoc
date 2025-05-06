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

        private Dictionary<string, string> plate_types = new Dictionary<string, string>()
            {
                {"ATmega328P (Arduino Uno, Nano, Pro Mini)", "atmega328p"},
                {"ATmega32U4 (Arduino Micro, Leonardo)", "atmega32u4" },
                {"ATmega2560 (Arduino Mega 2560)", "atmega2560" },
                {"ATmega1280 (Arduino Mega(старая))", "atmega1280" },
                {"ATtiny85", "attiny85" },
                {"ATtiny13", "attiny13" }
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
                }
            }

            foreach (string portname in SerialPort.GetPortNames())
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
            if (string.IsNullOrEmpty(Ports.Text) || string.IsNullOrEmpty(PlateType.Text))
            {
                MessageBox.Show("Select type of Arduino plate and COM-port", "Error", MessageBoxButton.OK);
            }
            else
            {
                ComboBoxItem selected_plate = (ComboBoxItem)PlateType.SelectedItem;
                string selected_plate_type = selected_plate.Content.ToString();

                ComboBoxItem selected_Port = (ComboBoxItem)Ports.SelectedItem;
                string selected_port = selected_Port.Content.ToString();

                string inputFile = @"CurrentProjRun\currentProjRun.cpp";
                string projBinFile = @"CurrentProjRun\currentProjRun.bin";
                string projHEXFile = @"CurrentProjRun\currentProjRun.hex";
                string compillerPath = @"sources\WinAVR-20100110\bin\avr-gcc.exe";
                string makingHEXutilPath = @"sources\WinAVR-20100110\bin\avr-objcopy.exe";
                string avrDudePath = @"sources\WinAVR-20100110\bin\avrdude.exe";
                string arduinoLibs = @"sources\arduino";
                string stdlibArduino = @"sources\avr-lib-c-main\include";
                string ArduinoStandart = @"sources\variants\standard";
                string additionalArgumennts = @" sources\arduino\wiring.c sources\arduino\wiring_digital.c sources\arduino\hooks.c";

                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile);

                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default)) /// реализовать проверку успешности завершегия процесса
                {
                    sw.Write(File);
                }

                bool FLAG = true;
                ProcessStartInfo processStartInfo1 = new ProcessStartInfo();
                processStartInfo1.FileName = compillerPath;
                processStartInfo1.Arguments = " -mmcu=" + selected_plate_type + " -Wall -g -Os -DF_CPU=16000000UL -o " + projBinFile + " " + inputFile + additionalArgumennts + " -I" + arduinoLibs + " -I" + ArduinoStandart + " -I" + stdlibArduino; // добавить выбор типа платы и подключать соответствующую библиотеку
                processStartInfo1.RedirectStandardOutput = true;
                processStartInfo1.RedirectStandardError = true;
                processStartInfo1.UseShellExecute = false;
                processStartInfo1.CreateNoWindow = true;

                Process compillingProcess = Process.Start(processStartInfo1);

                string output1 = compillingProcess.StandardOutput.ReadToEnd();
                string errors1 = compillingProcess.StandardError.ReadToEnd();

                ConsoleLine.Text = "Output: " + output1; /// сделать вывод этого аута в консоль для компорта (если она нужна)
                if (!string.IsNullOrEmpty(errors1))
                {
                    MessageBox.Show(errors1, "Error occured during compilation", MessageBoxButton.OK);
                    FLAG = false;
                }

                if (FLAG)
                {
                    if (compillingProcess.ExitCode != 0)
                    {
                        MessageBox.Show("Ошибка компиляции. Код завершения: " + compillingProcess.ExitCode, "Error of compilation", MessageBoxButton.OK);
                        FLAG = false;
                    }
                }
                compillingProcess.Close();


                if (FLAG)
                {
                    ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
                    processStartInfo2.FileName = makingHEXutilPath;
                    processStartInfo2.Arguments = " -j.text -j.data -O ihex " + projBinFile + " " + projHEXFile;
                    processStartInfo2.RedirectStandardOutput = true;
                    processStartInfo2.RedirectStandardError = true;
                    processStartInfo2.UseShellExecute = false;
                    processStartInfo2.CreateNoWindow = true;

                    Process makingHEXProcess = Process.Start(processStartInfo2);

                    string output2 = makingHEXProcess.StandardOutput.ReadToEnd();
                    string errors2 = makingHEXProcess.StandardError.ReadToEnd();

                    ConsoleLine.Text = "Output: " + output2; /// сделать вывод этого аута в консоль для компорта (если она нужна)
                    if (!string.IsNullOrEmpty(errors2))
                    {
                        MessageBox.Show(errors2, "Error occured", MessageBoxButton.OK);
                        FLAG = false;
                    }
                    else
                    {
                        if (makingHEXProcess.ExitCode != 0)
                        {
                            MessageBox.Show("Ошибка создания файла. Код завершения: " + makingHEXProcess.ExitCode, "Error of file creating", MessageBoxButton.OK);
                            FLAG = false;
                        }
                    }
                    makingHEXProcess.Close();
                }

                if (FLAG)
                {
                    ProcessStartInfo processStartInfo3 = new ProcessStartInfo();
                    processStartInfo3.FileName = avrDudePath;
                    processStartInfo3.Arguments = " -p " + selected_plate_type + "-c arduino -q -D -P " + selected_port + " -U flash:w:" + projHEXFile + ":i";
                    processStartInfo3.RedirectStandardOutput = true;
                    processStartInfo3.RedirectStandardError = true;
                    processStartInfo3.UseShellExecute = false;
                    processStartInfo3.CreateNoWindow = true;

                    Process boardFirmwareProcess = Process.Start(processStartInfo3);


                    string output3 = boardFirmwareProcess.StandardOutput.ReadToEnd();
                    string errors3 = boardFirmwareProcess.StandardError.ReadToEnd();

                    Console.WriteLine(errors3);

                    ConsoleLine.Text = "Output: " + output3; /// сделать вывод этого аута в консоль для компорта (если она нужна)
                    if (!string.IsNullOrEmpty(errors3))
                    {
                        MessageBox.Show(errors3, "Error occured durring flashing", MessageBoxButton.OK);
                        FLAG = false;
                    }
                    else
                    {
                        if (boardFirmwareProcess.ExitCode != 0)
                        {
                            MessageBox.Show("Ошибка прошивки. Код завершения: " + boardFirmwareProcess.ExitCode, "Error of flashing", MessageBoxButton.OK);
                            FLAG = false;
                        }
                    }
                    boardFirmwareProcess.Close();
                }
            }
        }
    }
}

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
using System.IO.Ports;
using System.Diagnostics;

namespace BleLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String comPort = "";
        SerialPort mySerialPort;
        private bool connected = false;
        private const bool debug = false;
        public MainWindow()
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                availablePorts.Items.Insert(0, port);
            }
               
            connStatus.Text = "Disconnected";
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mySerialPort.Close();
        }

        private void ButtonSwitch_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btnSwitch.Content == "Turn on")
            {
                mySerialPort.WriteLine("1");

            } else
            {
                mySerialPort.WriteLine("0");

            }

        }

        private void AvailablePorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(debug)
                Trace.WriteLine(availablePorts.SelectedItem.ToString());
           
        }

        private void ConnectSerial_Click(object sender, RoutedEventArgs e)
        {
   
            string selectedItem = availablePorts.SelectedItem.ToString();
            if (!selectedItem.Contains("Choose Serial Port"))
            {
                if (mySerialPort == null)
                {
                    mySerialPort = new SerialPort(selectedItem)
                    {
                        BaudRate = 9600,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None,
                        RtsEnable = true
                    };

                    mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    if(debug)
                        Trace.WriteLine("from object: ");

                    mySerialPort.Open();


                }
                //mySerialPort.Write("2");
                mySerialPort.WriteLine("2");

            } else
            {
                string messageBoxText = "You need to select a port first";
                string caption = "Can't connect to port";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            }
        }

        private void PerformLightSwitch(string connectionStatus, string buttonText, string lightBulbStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                connStatus.Text = connectionStatus;
                btnSwitch.IsEnabled = true;
                btnSwitch.Content = buttonText;
                lightBulbImage.Source = new BitmapImage(new Uri(lightBulbStatus, UriKind.Relative));
            });
        }

        private void PerformLightSwitch(string buttonText, string lightBulbStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                btnSwitch.Content = buttonText;
                lightBulbImage.Source = new BitmapImage(new Uri(lightBulbStatus, UriKind.Relative));
            });
        }

        private void DataReceivedHandler(
                    object sender,
                    SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            if (debug)
            {
                Trace.WriteLine("Data Received:");
                Trace.Write(indata);
            }

            if (indata == "1")
            {
                if (!connected)
                {
                    PerformLightSwitch("Connected", "Turn off", "images/lightbulb-on.png");
                    connected = true;
                }
                PerformLightSwitch("Turn off", "images/lightbulb-on.png");
            } else if (indata == "0")
            {
                if (!connected)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        PerformLightSwitch("Connected", "Turn on", "images/lightbulb-off.png");
                    });
                    connected = true;
                }

                this.Dispatcher.Invoke(() => {
                    PerformLightSwitch("Turn on", "images/lightbulb-off.png");
                });
            }

        }
    }
}

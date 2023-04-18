using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BleLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String comPort = "";
        SerialPort? mySerialPort;
        private bool connected = false;
        private const bool debug = false;
        public MainWindow()
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {

                var portVals = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                var portList = ports.Select(n => portVals.FirstOrDefault(s => s.Contains(n))).ToList();
                try
                {
                    foreach (string s in portList)
                    {
                        if (Regex.Replace(s, "\\s\\(.+\\)", "") == "USB Serial Port")
                        {
                            comPort = Regex.Match(s, "\\((.+)\\)").Groups[1].Value;
                        }
                    }
                }
                catch
                {
                    connStatus.Text = "Something went wrong";
                }

            }

            connStatus.Text = "Disconnected";
            availablePort.Text = "Available Port: " + comPort;
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

            }
            else
            {
                mySerialPort.WriteLine("0");

            }

        }

        private static void ShowWarningDialog(string message, string caption)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(message, caption, button, icon, MessageBoxResult.Yes);
        }

        private void ConnectSerial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mySerialPort == null)
                {

                    mySerialPort = new SerialPort(comPort)
                    {
                        BaudRate = 9600,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None,
                        RtsEnable = true
                    };

                    if (!mySerialPort.IsOpen)
                    {
                        mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                        if (debug)
                            Trace.WriteLine("from object: ");

                        mySerialPort?.Open();
                        mySerialPort?.WriteLine("2");
                        connectSerial.Content = "Disconnect";
                    }
                    else
                    {
                        ShowWarningDialog("Serial port already open. Close it first", "Serial Port Issue");
                        mySerialPort?.Close();
                    }
                } else
                {
                    mySerialPort?.Close();
                    mySerialPort = null;
                    connectSerial.Content = "Serial Connect";
                    SetConnectionState("Disconnected");
                }

            }
            catch (Exception Exception)
            {
                ShowWarningDialog("Unable to connect to port.", "Serial Port Issue");
                mySerialPort?.Close();
                mySerialPort = null;
            }

        }


        private void SetConnectionState(string connectionStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                connStatus.Text = connectionStatus;
                bool state = connectionStatus == "Connected";
                btnSwitch.IsEnabled = state;
                connected = state;

            });

        }
        private void PerformLightSwitch(string connectionStatus, string buttonText, string lightBulbStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                SetConnectionState("Connected");
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
            switch (indata)
            {
                case "0":
                    if (!connected)
                    {
                        PerformLightSwitch("Connected", "Turn on", "images/lightbulb-off.png");

                    }
                    PerformLightSwitch("Turn on", "images/lightbulb-off.png");
                    break;
                case "1":
                    if (!connected)
                    {
                        PerformLightSwitch("Connected", "Turn off", "images/lightbulb-on.png");

                    }
                    PerformLightSwitch("Turn off", "images/lightbulb-on.png");
                    break;
                case "+CONNECT":
                    SetConnectionState("Connected");
                    break;
                case "+DISCONNECT":
                    SetConnectionState("Disconnected");
                    break;
            }


        }
    }
}


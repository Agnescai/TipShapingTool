using System;
using System.Collections.Generic;
using System.IO.Ports;
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


namespace TipShaping
{
    /// <summary>
    /// Interaction logic for TrioControllerConnection.xaml
    /// </summary>
    public partial class BigTechControllerConnection : Window
    {

        private StepperMotorControl stepperMotorControl;

        public BigTechControllerConnection (StepperMotorControl motorControl)
        {
            InitializeComponent();
            LoadBaudRates();
            stepperMotorControl = motorControl;

        }
        private void BigTechControllerConnection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save the serial port name and its open/closed status before closing
            Properties.Settings.Default.SerialPortName = PortComboBox.SelectedItem?.ToString();
            Properties.Settings.Default.SerialPortStatus = stepperMotorControl.serialPort.IsOpen;
            Properties.Settings.Default.Save();
        }
        private void BigTechControllerConnection_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve the saved serial port name and its status
            string savedPortName = Properties.Settings.Default.SerialPortName;
            bool wasPortOpen = Properties.Settings.Default.SerialPortStatus;

            // Load the available ports again
            LoadAvailablePorts();

            //// Set the previously selected port in the ComboBox
            if (!string.IsNullOrEmpty(savedPortName) && PortComboBox.Items.Contains(savedPortName))
            {
                PortComboBox.SelectedItem = savedPortName;
            }

            // If the port was open previously, do nothing (port remains open)
            if (wasPortOpen && stepperMotorControl.serialPort.IsOpen)
            {
                ConnectButton.Content = "Disconnect";  // Port is open, show "Disconnect"
            }
            else if (!wasPortOpen || !stepperMotorControl.serialPort.IsOpen)
            {
                // If the port was not open or the current port is not open, ensure the button says "Connect"
                ConnectButton.Content = "Connect";
            }
        }



        private void LoadAvailablePorts()
        {
            // Clear existing items
            PortComboBox.Items.Clear();

            // Get all available COM ports
            string[] ports = SerialPort.GetPortNames();

            // Add each port to the ComboBox
            foreach (string port in ports)
            {
                PortComboBox.Items.Add(port);
            }

            //// Select the first port by default (optional)
            //if (PortComboBox.Items.Count > 0)
            //{
            //    PortComboBox.SelectedIndex = 0;
            //}
            PortComboBox.SelectedItem="COM7";


        }

        private void LoadBaudRates()
        {
            // List of commonly used baud rates
            int[] baudRates = { 9600, 19200, 38400, 57600, 115200 };

            BaudRateComboBox.Items.Clear();

            // Add baud rates to the ComboBox
            foreach (int baudRate in baudRates)
            {
                BaudRateComboBox.Items.Add(baudRate);
            }

            // Set a default selection (optional)
            BaudRateComboBox.SelectedItem = 115200;
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (BaudRateComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a baud rate.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string selectedPort = PortComboBox.SelectedItem.ToString();
                int baudRate = (int)BaudRateComboBox.SelectedItem;  // Cast the selected item to an int

                // If the port is already open, close it before changing the port name or opening a new one
                if (stepperMotorControl.serialPort.IsOpen)
                {


                    // Close the port first
                    //stepperMotorControl.serialPort.PortName = selectedPort; // Replace with the actual port name
                    //stepperMotorControl.serialPort.BaudRate = baudRate;  // Replace with the desired baud rate

                    stepperMotorControl.CloseSerialPort(selectedPort);
                    MessageBox.Show("Disconnected");
                    ConnectButton.Content = "Connect";

                    // Save the port and status after closing
                    Properties.Settings.Default.SerialPortName = selectedPort;
                    Properties.Settings.Default.SerialPortStatus = false;
                    Properties.Settings.Default.Save();
                }
                else
                {

                    // If the port is closed, open it
                    
                    stepperMotorControl.OpenSerialPort(selectedPort,baudRate);
                    MessageBox.Show("Connected to " + selectedPort);
                    ConnectButton.Content = "Disconnect";

                    // Save the port and status after opening
                    Properties.Settings.Default.SerialPortName = selectedPort;
                    Properties.Settings.Default.SerialPortStatus = true;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }



        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandInput.Text;
            if (command != "")
            // Send the command 
            { stepperMotorControl.SendCommand(command); }

        }

        
    }
}

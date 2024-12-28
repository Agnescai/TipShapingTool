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
using System.IO.Ports;

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
            LoadAvailablePorts();
            LoadBaudRates();
            stepperMotorControl = motorControl;

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

            // Select the first port by default (optional)
            if (PortComboBox.Items.Count > 0)
            {
                PortComboBox.SelectedIndex = 0;
            }
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

                if (!stepperMotorControl.serialPort.IsOpen)
                {
                    

                    if (selectedPort != null)
                    {
                        stepperMotorControl.OpenSerialPort(selectedPort, baudRate);
                        MessageBox.Show("Connected to " + selectedPort);
                        ConnectButton.Content = "Disconnect";
                    }
                    else
                    {
                        MessageBox.Show("Serial Port in null!");
                    }
                }
                else
                {
                    stepperMotorControl.CloseSerialPort(selectedPort);
                    MessageBox.Show("Disconnected");
                    ConnectButton.Content = "Connect";
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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

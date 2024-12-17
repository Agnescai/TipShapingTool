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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TipShaping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        // Define the field
        private bool[] isAxisEnabled = { false, false, false, false, false, false };
        string[] Max_Pos = {"100","100","50","6", "14", "36000" }; //forward travel limit SA,SF 13mm 30mm
        string[] Min_Pos = {"-100", "-100", "-50","-6", "-14", "-36000" }; //reverse travel limit
        private string SAlastPosition = string.Empty;
        private string SFlastPosition = string.Empty;
        private string LlastPosition = string.Empty;
        private const double Threshold = 0.001;  // Example: 0.001 mm as the threshold
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();
            LoadBaudRates();
           
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
                if (serialPort == null || !serialPort.IsOpen)
                {
                    string selectedPort = PortComboBox.SelectedItem.ToString();

                    int baudRate = (int)BaudRateComboBox.SelectedItem;  // Cast the selected item to an int


                    serialPort = new SerialPort(selectedPort, baudRate);
                    //serialPort = new SerialPort("COM4", 115200);
                    
                    serialPort.Open();
                    serialPort.DtrEnable = true;
                    serialPort.RtsEnable = true;
                    StartMonitoringPosition(); //start monitoring the current position of the axes
                    serialPort.DataReceived += SerialPort_DataReceived;
                    MessageBox.Show("Connected to " + selectedPort);
                    ConnectButton.Content = "Disconnect";
                }
                else
                {
                    StopMonitoringPosition();
                    serialPort.Close();
                    MessageBox.Show("Disconnected");
                    ConnectButton.Content = "Connect";
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private void EnableXButton_Click(object sender, RoutedEventArgs e)
        {
            //X:0,Y:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            isAxisEnabled[0] = !isAxisEnabled[0];


            // Update the button content
            EnableXButton.Content = isAxisEnabled[0] ? "Disable X" : "Enable X";

            // Perform additional actions based on the state
            if (isAxisEnabled[0])
            {
                MessageBox.Show("X Enabled");
                // Add code to enable X
            }
            else
            {
                MessageBox.Show("X Disabled");
                // Add code to disable X
            }
        }

        private void EnableYButton_Click(object sender, RoutedEventArgs e)
        {
            //X:0,Y:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            isAxisEnabled[1] = !isAxisEnabled[1];

            // Update the button content
            EnableYButton.Content = isAxisEnabled[1] ? "Disable Y" : "Enable Y";

            // Perform additional actions based on the state
            if (isAxisEnabled[1])
            {
                MessageBox.Show("Y Enabled");
                // Add code to enable Y
            }
            else
            {
                MessageBox.Show("Y Disabled");
                // Add code to disable Y
            }
        }

        private void EnableZButton_Click(object sender, RoutedEventArgs e)
        {
            //X:0,Y:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            isAxisEnabled[2] = !isAxisEnabled[2];


            // Update the button content
            EnableZButton.Content = isAxisEnabled[2] ? "Disable Z" : "Enable Z";

            // Perform additional actions based on the state
            if (isAxisEnabled[2])
            {
                MessageBox.Show("Z Enabled");
                // Add code to enable Z
            }
            else
            {
                MessageBox.Show("Z Disabled");
                // Add code to disable Z
            }
        }

        private void EnableSAButton_Click(object sender, RoutedEventArgs e)
        {
            // Axis mapping: X:0, Y:1, Z:2, SA:3, SF:4, L:5
            // Toggle the state for SA
            isAxisEnabled[3] = !isAxisEnabled[3];

            // Update the button content
            EnableSAButton.Content = isAxisEnabled[3] ? "Disable SA" : "Enable SA";

            // Send G-code to enable or disable the axis
            if (isAxisEnabled[3])
            {
                // Send G-code to enable the corresponding stepper motor (X-axis for S1)
                string enableCommand = "M17 X"; // M17 is the Marlin G-code to engage stepper motors
                SendCommand(enableCommand);
                MessageBox.Show("SA Enabled");
            }
            else
            {
                // Send G-code to disable the corresponding stepper motor
                string disableCommand = "M18 X"; // M18 disengages the stepper motor
                SendCommand(disableCommand);
                MessageBox.Show("SA Disabled");
            }
        }

        private void EnableSFButton_Click(object sender, RoutedEventArgs e)
        {
            //X:0,Y:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            isAxisEnabled[4] = !isAxisEnabled[4];


            // Update the button content
            EnableSFButton.Content = isAxisEnabled[4] ? "Disable SF" : "Enable SF";

            // Perform additional actions based on the state
            if (isAxisEnabled[4])
            {
                // Send G-code to enable the corresponding stepper motor (X-axis for S1)
                string enableCommand = "M17 Y"; // M17 is the Marlin G-code to engage stepper motors
                SendCommand(enableCommand);
                MessageBox.Show("SF Enabled");
            }
            else
            {
                // Send G-code to disable the corresponding stepper motor
                string disableCommand = "M18 Y"; // M18 disengages the stepper motor
                SendCommand(disableCommand);
                MessageBox.Show("SF Disabled");
            }
            
        }

        private void EnableLButton_Click(object sender, RoutedEventArgs e)
        {
            //X:0,Y:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            isAxisEnabled[5] = !isAxisEnabled[5];


            // Update the button content
            EnableLButton.Content = isAxisEnabled[5] ? "Disable L" : "Enable L";

            // Perform additional actions based on the state
            if (isAxisEnabled[5])
            {
                // Send G-code to enable the corresponding stepper motor 
                string enableCommand = "M17 Z"; // M17 is the Marlin G-code to engage stepper motors
                SendCommand(enableCommand);
                MessageBox.Show("L Enabled");
            }
            else
            {
                // Send G-code to disable the corresponding stepper motor
                string disableCommand = "M18 Z"; // M18 disengages the stepper motor
                SendCommand(disableCommand);
                MessageBox.Show("L Disabled");
            }
        }



        private int GetAxisIndex(string axisName)
        {
            return axisName switch
            {
                "X" => 0,
                "Y" => 1,
                "Z" => 2,
                "SA" => 3,
                "SF" => 4,
                "L" => 5,
                _ => -1 // Invalid axis
            };
        }



        private string GenerateGCode(string axisName, string velocity, string distance, string moveType, bool forward)
        {
            string gCode = string.Empty;

            string axis = axisName switch
            {
                "SA" => "X",
                "SF" => "Y",
                "L" => "Z",
                _ => throw new ArgumentException("Invalid axis name.") // Handle unknown axis names
            };


            string velocityInMMPerMinInString = "0";

            if (double.TryParse(velocity, out double velocityInMMPerSec))
            {
                // Convert from mm/s to mm/min
                double velocityInMMPerMin = velocityInMMPerSec * 60;

                // Convert the result back to string
                velocityInMMPerMinInString = velocityInMMPerMin.ToString(); // Converts to string
               
            }
            else
            {
                
                throw new ArgumentException("Invalid velocity input.");
            }

            switch (moveType)
            {
                case "Relative Move":
                    gCode = "G91\n";
                    break;
                case "Absolute Move":
                    gCode = "G90\n";
                    break;
                case "Jog":
                    gCode = "G90\n"; //go to the travel limit?
                    break;
                default:
                    throw new ArgumentException("Unknown move type");
            }

            var direction = forward ? distance : $"-{distance}";
            gCode += $" G0 {axis}{direction} F{velocityInMMPerMinInString}";
            return gCode;
        }

        private void SendCommand(string command)
        {
            // This function should send the GCode to the Marlin controller.
           // MessageBox.Show($"Sending Command: {command}");
            //// Implement communication with Marlin (e.g., via serial port).
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(command); // Send the G-code command to the connected device
            }
            else
            {
                MessageBox.Show("Serial port is not connected or open.");
            }

        }

        private void ForwardButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var velocity = VelocityInput.Text;
                var distance = DistanceInput.Text;
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

                int axisIndex = GetAxisIndex(axisName);
                if (axisIndex == -1)
                {
                    MessageBox.Show("Invalid axis selected.");
                    return;
                }

                // Check if the axis is enabled
                if (!isAxisEnabled[axisIndex])
                {
                    MessageBox.Show($"Enable the {axisName} axis first.");
                    return;
                }

                if (moveType == "Jog")
                {
                    if (string.IsNullOrEmpty(velocity) || string.IsNullOrEmpty(axisName) || string.IsNullOrEmpty(moveType))
                    {
                        MessageBox.Show("Please fill out all fields.");
                        return;
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(velocity) || string.IsNullOrEmpty(distance) || string.IsNullOrEmpty(axisName) || string.IsNullOrEmpty(moveType))
                    {
                        MessageBox.Show("Please fill out all fields.");
                        return;
                    }

                }



                if (axisName == "SA" || axisName == "SF" )
                {
                    if (moveType == "Jog")
                    {
                        double.TryParse(Max_Pos[axisIndex], out double MaxJogPos);
                        MaxJogPos = MaxJogPos - 0.1;
                        distance = MaxJogPos.ToString();
                    }
                    var command = GenerateGCode(axisName, velocity, distance, moveType, forward: true);
                    SendCommand(command);
                }
                else
                {
                    if (axisName == "L")
                    {
                        if(moveType == "Jog")
                        {
                            double.TryParse(Max_Pos[axisIndex], out double MaxJogPos);
                            MaxJogPos = MaxJogPos - 0.1;
                            distance = MaxJogPos.ToString();
                        }
                        if (double.TryParse(distance, out double distanceDeg))
                        {
                            double distanceRev = distanceDeg / 360;

                            var command = GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, forward: true);
                            SendCommand(command);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid number."); 
                        }
                        
                    }

                }



            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }

        private void ForwardButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (moveType == "Jog")
            {
                // Stop jogging
                SendCommand("M410"); //  stop command
            }

        }

        private void ReverseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
           try
            { 
                var velocity = VelocityInput.Text;
                var distance = DistanceInput.Text;
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

                int axisIndex = GetAxisIndex(axisName);
                if (axisIndex == -1)
                {
                    MessageBox.Show("Invalid axis selected.");
                    return;
                }

                // Check if the axis is enabled
                if (!isAxisEnabled[axisIndex])
                {
                    MessageBox.Show($"Enable the {axisName} axis first.");
                    return;
                }


                if (moveType == "Jog")
                {
                    if (string.IsNullOrEmpty(velocity) || string.IsNullOrEmpty(axisName) || string.IsNullOrEmpty(moveType))
                    {
                        MessageBox.Show("Please fill out all fields.");
                        return;
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(velocity) || string.IsNullOrEmpty(distance) || string.IsNullOrEmpty(axisName) || string.IsNullOrEmpty(moveType))
                    {
                        MessageBox.Show("Please fill out all fields.");
                        return;
                    }

                }

                if (axisName == "SA" || axisName == "SF")
                { //add code to check that 
                    if (moveType == "Jog")
                    {
                        double.TryParse(Min_Pos[axisIndex], out double MinJogPos);
                        MinJogPos = Math.Abs(MinJogPos) - 0.1;
                        distance = MinJogPos.ToString();
                    }
                    var command = GenerateGCode(axisName, velocity, distance, moveType, forward: false);
                    SendCommand(command);
                }
                else
                {
                    if (axisName == "L")
                    {
                        if (moveType == "Jog")
                        {
                            double.TryParse(Min_Pos[axisIndex], out double MinJogPos);
                            MinJogPos = Math.Abs(MinJogPos) - 0.1;
                            distance = MinJogPos.ToString();
                        }
                        if (double.TryParse(distance, out double distanceDeg))
                        {
                            double distanceRev = distanceDeg / 360;

                            var command = GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, forward: false);
                            SendCommand(command);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid number.");
                        }

                    }

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }

       

        private void ReverseButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (moveType == "Jog")
            {
                // Stop jogging
                SendCommand("M410"); // Replace with your controller's specific stop command

            }
    }

        private void HomeXButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HomeYButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HomeZButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HomeSAButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAxisEnabled[3]) 
            { 
                MessageBox.Show("Enable the axis SA first.");
                return;
            }else
            { 
                SendCommand("G28 X");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                SendCommand("G91\n G0 X7 F120");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                SendCommand("G92 X0");
            }
           
        }

        private void HomeSFButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAxisEnabled[4])
            {
                MessageBox.Show("Enable the axis SF first.");
                return;
            }
            else
            {
                SendCommand("G28 Y");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                SendCommand("G91\n G0 Y15 F120");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                SendCommand("G92 Y0");
            }
        }

        private void HomeLButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAxisEnabled[5])
            {
                MessageBox.Show("Enable the axis L first.");
                return;
            }
            else
            {
                //SendCommand("G28 Y");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                SendCommand("G92 Z0");
            }

        }



        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandInput.Text;
            if (command != "")
            // Send the command 
            { SendCommand(command); }

        }

        /**********Detect position every 1s*******************/
        // Start continuous position reporting
        private async void StartMonitoringPosition()
        {
            // wait for 1 seconds to make sure serial port is open
            await Task.Delay(1000);
            // Send M154 command to start reporting positions every 1 second
            //SendCommand("M154 S1");
            SendCommand("M114 R");

            // Optionally, can also use a timer for periodic status checks (in case of slower responses)
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000) // 1 second interval
            };
            timer.Tick += (s, e) => SendCommand("M114 R");
            timer.Start();
        }

        // Stop continuous reporting
        private void StopMonitoringPosition()
        {
            // Stop the timer to prevent further M114 commands
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= (s, e) => SendCommand("M114 R"); // Unsubscribe from the event
                timer = null;
            }

            // Optionally close the serial port if required
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        static float ExtractValue(Regex regex, string input)
        {
            Match match = regex.Match(input);
            if (match.Success)
            {
                return float.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new Exception("Value not found.");
            }
        }

        // Handle incoming data from the serial port
        // Handle incoming data from the serial port  X:-6.50 Y:2.13 Z:-2685.00 E:0.01 Count X:-20800 Y:6817 Z:-8592000
        // Handle incoming data from the serial port
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the line of data received from the serial port
                string data = serialPort.ReadLine().Trim();  // Trim any unnecessary whitespace or newline characters
                Debug.WriteLine($"Received Data: {data}");

                if (data.StartsWith("X:"))
                {
                    // Regular expressions for X, Y, and Z values
                    Regex regexX = new Regex(@"X:\s*(-?\d+(\.\d+)?)");
                    Regex regexY = new Regex(@"Y:\s*(-?\d+(\.\d+)?)");
                    Regex regexZ = new Regex(@"Z:\s*(-?\d+(\.\d+)?)");

                    // Extract X, Y, and Z values
                    float xValue = ExtractValue(regexX, data);
                    float yValue = ExtractValue(regexY, data);
                    float zValue = ExtractValue(regexZ, data);

                    Dispatcher.Invoke(() => UpdatePositionDisplay(xValue.ToString(), yValue.ToString(), zValue.ToString()));

                    if (SAlastPosition=="" || SFlastPosition == "" || LlastPosition == "" )
                    {
                        SAlastPosition = xValue.ToString();
                        SFlastPosition = yValue.ToString();
                        LlastPosition = zValue.ToString();
                    }


                    // Update the UI to show "Moving"
                    if (HasSignificantMovement(xValue.ToString(), SAlastPosition, Threshold) ||
                        HasSignificantMovement(yValue.ToString(), SFlastPosition, Threshold) ||
                        HasSignificantMovement(zValue.ToString(), LlastPosition, Threshold))
                    {
                        // Update last known positions if significant movement is detected
                        SAlastPosition = xValue.ToString();
                        SFlastPosition = yValue.ToString();
                        LlastPosition = zValue.ToString();



                        // Update the UI to show "Moving"
                        Dispatcher.Invoke(() => UpdateStatusDisplay("Moving"));
                    }
                    else
                    {
                        // If no significant movement, show "Idle"
                        Dispatcher.Invoke(() => UpdateStatusDisplay("Idle"));
                    }

                }

                
            }
            catch (Exception ex)
            {
                // If there's an error reading from the serial port, show an error message
                Dispatcher.Invoke(() => MessageBox.Show($"Error reading from serial port: {ex.Message}"));
            }
        }




        // check if the movement is significant
        private bool HasSignificantMovement(string currentPos, string lastPos, double threshold) 
        {
            // Convert the position strings to double values
            double currentPosition = double.Parse(currentPos);
            double lastPosition = double.Parse(lastPos);

            // Check if the difference between current and last position exceeds the threshold
            return Math.Abs(currentPosition - lastPosition) > threshold;
        }

        // Update the UI with the current status
        private void UpdateStatusDisplay(string status)
        {
            AxisStatus.Text = status;

        }

        // Extract the position for a specific axis from the data string
        private string ExtractAxisPosition(string data, string axis)
        {
            try
            {
                // Log the data for debugging
                Debug.WriteLine($"Received Data: {data}");

                // Regex pattern to extract the axis position as a number (including potential decimals or negative signs)
                string pattern = $@"{axis}([\-0-9.]+)";

                Regex regex = new Regex(pattern);
                Match match = regex.Match(data);

                // If the axis is found in the data
                if (match.Success)
                {
                    // Return the extracted position value
                    Debug.WriteLine($"{axis} Position Extracted: {match.Groups[1].Value}");
                    return match.Groups[1].Value;
                }
                else
                {
                    // Log if the axis was not found
                    Debug.WriteLine($"Axis {axis} not found in data.");
                    return "N/A"; // Or return an appropriate default value
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                Debug.WriteLine($"Error extracting axis position: {ex.Message}");
                return "Error"; // Return an error message if something goes wrong
            }
        }



        // Update the UI to display the current X, Y, Z positions
        private void UpdatePositionDisplay(string x, string y, string z)
        {
            if (double.TryParse(x, out double xValue))
            {
                SAMPosTextBox.Text = xValue.ToString("F3"); // Format with 3 decimal places
            }
            else
            {
                MessageBox.Show("Invalid X value. Please enter a valid number.");
            }

            if (double.TryParse(y, out double yValue))
            {
                SFMPosTextBox.Text = yValue.ToString("F3"); // Format with 3 decimal places
            }
            else
            {
                MessageBox.Show("Invalid Y value. Please enter a valid number.");
            }

            if (double.TryParse(z, out double zValue))
            {
                double result = zValue * 360;
                LMPosTextBox.Text = result.ToString("F3"); // Format with 3 decimal places
            }
            else
            {
                MessageBox.Show("Invalid Z value. Please enter a valid number.");
            }
        }

        // Window closing event to stop monitoring
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopMonitoringPosition();
        }

        private void StopAllMotion_Click(object sender, RoutedEventArgs e)
        {

            SendCommand("M410"); //Quickstop
        }

        private void AxisSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AxisSelection.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content.ToString();
                DistanceUnit.Text = content == "L" ? "deg" : "mm";
                VelocityTextBlock.Text = content == "L" ? "Velocity(rev/s):" : "Verevlocity(mm/s):";
            }

            HandleRotationButtonsVisibility();
        }

        private void RotationButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (sender is Button button)
            {
                DistanceInput.Text = button.Content.ToString();
            }
           
        }

        private void MoveTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleRotationButtonsVisibility();
        }

        private void HandleRotationButtonsVisibility()
        {
            bool isAxisL = AxisSelection.SelectedItem is ComboBoxItem axisItem && axisItem.Content.ToString() == "L";
            bool isNotJog = MoveTypeSelection.SelectedItem is ComboBoxItem moveTypeItem && moveTypeItem.Content.ToString() != "Jog";

            RotationButtonsPanel.Visibility = isAxisL && isNotJog ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Ports;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace TipShaping
{
    public class StepperMotorControl
    {
        public SerialPort serialPort = new SerialPort();
        float SAlastPosition = 0;
        float SFlastPosition = 0;
        float LlastPosition = 0;
        bool isInitial = true;
        private const float Threshold = 0.001f;  // Example: 0.001 mm as the threshold
        System.Timers.Timer timer;

        public delegate void PositionUpdateHandler(float x, float y, float z);
        public event PositionUpdateHandler PositionUpdated;

        public delegate void MovingStatusUpdateHandler(bool Moving);
        public event MovingStatusUpdateHandler MovingStatusUpdated;


        public StepperMotorControl()
        {

            serialPort.DataReceived += SerialPort_DataReceived;
        }
        public void OpenSerialPort(string selectedPort, int baudRate)
        {
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.PortName = selectedPort;
            serialPort.BaudRate = baudRate;
            serialPort.Open();
            StartMonitoringPosition(); //start monitoring the current position of the axes
        }
        public void CloseSerialPort(string selectedPort)
        {
            serialPort.PortName = selectedPort;
            StopMonitoringPosition();
            serialPort.Close();
        }


        public void SendCommand(string command)
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

        /**********Detect position every 1s*******************/
        // Start continuous position reporting
        public async Task StartMonitoringPosition()
        {
            // Wait for 1 second to ensure the serial port is open
            await Task.Delay(1000);

            // Send M114 R command to start reporting positions every second
            SendCommand("M114 R");

            // Initialize and configure the timer
            timer = new System.Timers.Timer
            {
                Interval = 1000, // 1 second in milliseconds
                AutoReset = true // Ensures the timer triggers repeatedly
            };

            timer.Elapsed += (s, e) => SendCommand("M114 R");

            // Start the timer
            timer.Start();
        }

        // Ensure the timer is disposed properly when no longer needed
        public void StopMonitoringPosition()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
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

                // Raise the PositionUpdated event on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PositionUpdated?.Invoke(xValue, yValue, zValue);
                });

                //Dispatcher.Invoke(() => UpdatePositionDisplay(xValue.ToString(), yValue.ToString(), zValue.ToString()));

                if (isInitial)
                {
                    SAlastPosition = xValue;
                    SFlastPosition = yValue;
                    LlastPosition = zValue;
                    isInitial = false;
                }


                // Update the UI to show "Moving"
                if (HasSignificantMovement(xValue, SAlastPosition, Threshold) ||
                    HasSignificantMovement(yValue, SFlastPosition, Threshold) ||
                    HasSignificantMovement(zValue, LlastPosition, Threshold))
                {
                    // Update last known positions if significant movement is detected
                    SAlastPosition = xValue;
                    SFlastPosition = yValue;
                    LlastPosition = zValue;



                    // Update the UI to show "Moving"
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MovingStatusUpdated?.Invoke(true);
                    });
                    // Dispatcher.Invoke(() => UpdateStatusDisplay("Moving"));
                }
                else
                {
                    // If no significant movement, show "Idle"
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MovingStatusUpdated?.Invoke(false);
                    });
                    // Dispatcher.Invoke(() => UpdateStatusDisplay("Idle"));
                }

            }

        }

        public string GenerateGCode(string axisName, string velocity, string distance, string moveType, bool forward)
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



        // check if the movement is significant
        private bool HasSignificantMovement(float currentPos, float lastPos, float threshold)
        {
            // Check if the difference between current and last position exceeds the threshold
            return Math.Abs(currentPos - lastPos) > threshold;
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
    }
}

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
using System.Diagnostics.Eventing.Reader;
using Windows.Storage.Streams;


namespace TipShaping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Define the field
        private bool[] isAxisEnabled = { false, false, false, false, false, false };
        double[] isAxisEnabledD = { 0, 0, 0, 0, 0, 0 };
        double[] isDriveEnabledD = { 0, 0, 0, 0, 0, 0 };
        string[] Max_Pos = { "100", "100", "20", "6", "14", "36000" }; //forward travel limit SA,SF 13mm 30mm
        string[] Min_Pos = { "-100", "-100", "-20", "-6", "-14", "-36000" }; //reverse travel limit
                                                                             //private string SAlastPosition = string.Empty;
                                                                             //private string SFlastPosition = string.Empty;
                                                                             //private string LlastPosition = string.Empty;


        private CameraSetting cameraSettingWindow;
        private BigTechControllerConnection bigTechControllerConnectionWindow;
        private TrioControllerConnection trioControllerConnectionWindow;
        private StepperMotorControl stepperMotorControl;
        private TrioMotionControl trioMotionControl;

        public MainWindow()
        {
            // Get the screen dimensions
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            // Set the MainWindow position (lower half of the screen)
            this.Left = 0; // Start at the left edge
            this.Top = screenHeight / 2; // Start in the middle of the screen

            InitializeComponent();
            stepperMotorControl = new StepperMotorControl();


            stepperMotorControl.PositionUpdated += StepperMotorOnPositionUpdated;
            stepperMotorControl.MovingStatusUpdated += StepperMotorOnMovingStatusUpdated;

            trioMotionControl = new TrioMotionControl();

        }

        private void StepperMotorOnPositionUpdated(float x, float y, float z)
        {
            // Update the UI
            SAMPosTextBox.Text = $"{x:F3}";
            SFMPosTextBox.Text = $"{y:F3}";
            LMPosTextBox.Text = $"{z:F3}";
        }
        private void StepperMotorOnMovingStatusUpdated(bool Moving)
        {
            // Update the UI
            if (Moving)
            {
                AxisStatus.Text = "Moving";
            }
            else
            { AxisStatus.Text = "Idle"; }

        }




        private void EnableXButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("X");
            bool Connected = trioMotionControl.IsControllerConnected();
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.DRIVE_ENABLE, AxisIndex, out isDriveEnabledD[AxisIndex]);

            if (Connected & isDriveEnabledD[AxisIndex] == 1.0)
            {
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);


                if (!isAxisEnabled[AxisIndex])
                {
                    MessageBox.Show("X Enabled");
                    // Add code to enable X
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                }
                else
                {
                    MessageBox.Show("X Disabled");
                    // Add code to disable X
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                }


                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableYButton.Content = isAxisEnabled[AxisIndex] ? "Disable X" : "Enable X";
            }
            else
            {
                MessageBox.Show("Trio controller is not connected or drive is not enabled!");
            }


        }

        private void EnableYButton_Click(object sender, RoutedEventArgs e)
        {
            //Y:0,X:1,Z:2,SA:3,SF:4,L:5
            // Toggle the state
            int AxisIndex = GetAxisIndex("Y");
            bool Connected= trioMotionControl.IsControllerConnected();
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.DRIVE_ENABLE, AxisIndex, out isDriveEnabledD[AxisIndex]);

            if (Connected & isDriveEnabledD[AxisIndex]==1.0 )
            {
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);


                if (!isAxisEnabled[AxisIndex])
                {
                    MessageBox.Show("Y Enabled");
                    // Add code to enable Y
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                }
                else
                {
                    MessageBox.Show("Y Disabled");
                    // Add code to disable Y
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                }


                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableYButton.Content = isAxisEnabled[AxisIndex] ? "Disable Y" : "Enable Y";
            }
            else
            {
                MessageBox.Show("Trio controller is not connected or drive is not enabled!");
            }

           
        }

        private void EnableZButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("Z");
            bool Connected = trioMotionControl.IsControllerConnected();
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.DRIVE_ENABLE, AxisIndex, out isDriveEnabledD[AxisIndex]);

            if (Connected & isDriveEnabledD[AxisIndex] == 1.0)
            {
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);


                if (!isAxisEnabled[AxisIndex])
                {
                    MessageBox.Show("Z Enabled");
                    // Add code to enable Z
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                }
                else
                {
                    MessageBox.Show("Z Disabled");
                    // Add code to disable Z
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                }


                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableYButton.Content = isAxisEnabled[AxisIndex] ? "Disable Z" : "Enable Z";
            }
            else
            {
                MessageBox.Show("Trio controller is not connected or drive is not enabled!");
            }


        }

        private void EnableSAButton_Click(object sender, RoutedEventArgs e)
        {
            
            

            if (stepperMotorControl.serialPort.IsOpen)
            {
                int AxisIndex = GetAxisIndex("SA");
                // Send G-code to enable or disable the axis
                if (!isAxisEnabled[AxisIndex])
                {
                    // Send G-code to enable the corresponding stepper motor (X-axis for S1)
                    string enableCommand = "M17 X"; // M17 is the Marlin G-code to engage stepper motors
                    stepperMotorControl.SendCommand(enableCommand);
                    MessageBox.Show("SA Enabled");
                }
                else
                {
                    // Send G-code to disable the corresponding stepper motor
                    string disableCommand = "M18 X"; // M18 disengages the stepper motor
                    stepperMotorControl.SendCommand(disableCommand);
                    MessageBox.Show("SA Disabled");
                }
                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];

                // Update the button content
                EnableSAButton.Content = isAxisEnabled[AxisIndex] ? "Disable SA" : "Enable SA";
            }
            else
            {
                MessageBox.Show("BigTech Controller is not connected!");
            }

           
        }

        private void EnableSFButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (stepperMotorControl.serialPort.IsOpen)
            {
                int AxisIndex = GetAxisIndex("SF");

                if (!isAxisEnabled[AxisIndex])
                {
                    // Send G-code to enable the corresponding stepper motor (X-axis for S1)
                    string enableCommand = "M17 Y"; // M17 is the Marlin G-code to engage stepper motors
                    stepperMotorControl.SendCommand(enableCommand);
                    MessageBox.Show("SF Enabled");
                }
                else
                {
                    // Send G-code to disable the corresponding stepper motor
                    string disableCommand = "M18 Y"; // M18 disengages the stepper motor
                    stepperMotorControl.SendCommand(disableCommand);
                    MessageBox.Show("SF Disabled");
                }
                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableSFButton.Content = isAxisEnabled[AxisIndex] ? "Disable SF" : "Enable SF";
            }
            else
            {
                MessageBox.Show("BigTech Controller is not connected!");
            }

          
        }

        private void EnableLButton_Click(object sender, RoutedEventArgs e)
        {
           

            if (stepperMotorControl.serialPort.IsOpen)
            {
                int AxisIndex = GetAxisIndex("L");
                if (!isAxisEnabled[AxisIndex])
                {
                    // Send G-code to enable the corresponding stepper motor 
                    string enableCommand = "M17 Z"; // M17 is the Marlin G-code to engage stepper motors
                    stepperMotorControl.SendCommand(enableCommand);
                    MessageBox.Show("L Enabled");
                }
                else
                {
                    // Send G-code to disable the corresponding stepper motor
                    string disableCommand = "M18 Z"; // M18 disengages the stepper motor
                    stepperMotorControl.SendCommand(disableCommand);
                    MessageBox.Show("L Disabled");
                }
                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableLButton.Content = isAxisEnabled[AxisIndex] ? "Disable L" : "Enable L";
            }
            else
            { MessageBox.Show("BigTech Controller is not connected!"); }

                

        }



        private int GetAxisIndex(string axisName)
        {
            return axisName switch
            {
                "Y" => 0,
                "X" => 1,
                "Z" => 2,
                "SA" => 3,
                "SF" => 4,
                "L" => 5,
                _ => -1 // Invalid axis
            };
        }






        private void ForwardButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var velocity = VelocityInput.Text;
                var distance = DistanceInput.Text;
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                int axisIndex = -1;
                if (axisName =="XY")
                {
                    axisName = "X"; //this button is for X
                    axisIndex = GetAxisIndex(axisName);
                    trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, axisIndex, out isAxisEnabledD[axisIndex]);
                    isAxisEnabled[axisIndex] = (isAxisEnabledD[axisIndex] == 1);
                }

                axisIndex = GetAxisIndex(axisName);//save for other axis
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

                if (axisName == "X")// trio
                {
   
                    double distanceValue = double.Parse(distance);
                    // Prepare parameters for the MoveRel function
                    double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                    int baseAxis = axisIndex;                            // Base axis for movement
                    double velocityValue = double.Parse(velocity);
                    trioMotionControl.SetMotion(baseAxis, velocityValue);//can also set Accel, Decel, Jerk

                    if (moveType == "Jog")
                    {
                        //trioMotionControl.Base();
                        bool JogSuccessful = trioMotionControl.Forward(baseAxis);
                        if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                        else { Debug.Print("JogSuccessful: 0"); }
                    }

                    if (moveType == "Relative Move")
                    {

                        // Call MoveRel
                        bool success = trioMotionControl.MoveRel(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move on X axis completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move on X axis failed.");
                        }

                    }

                    if (moveType == "Absolute Move")
                    {
                        // Call MoveAbs
                        bool success = trioMotionControl.MoveAbs(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move on X axis completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move on X axis failed.");
                        }

                    }

                }




                if (axisName == "L")
                {
                    if (moveType == "Jog")
                    {
                        double.TryParse(Max_Pos[axisIndex], out double MaxJogPos);
                        MaxJogPos = MaxJogPos - 0.1;
                        distance = MaxJogPos.ToString();
                    }
                    if (double.TryParse(distance, out double distanceDeg))
                    {
                        double distanceRev = distanceDeg / 360;

                        var command = stepperMotorControl.GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, positive: true);
                        stepperMotorControl.SendCommand(command);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number.");
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
            var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (axisName == "XY")
            {
                axisName = "X"; //this button is for X
            }
            int axisIndex = GetAxisIndex(axisName);

            if (moveType == "Jog")
            {
                // Stop jogging
                if (axisName == "X") //X axis
                {
                    
                    trioMotionControl.Cancel(0, axisIndex);//.0 cancels the current move on the base axis, 1 cancels the buffered moves on the base axis.

                }
                else if (axisName == "L") //L axis
                {
                    stepperMotorControl.SendCommand("M410"); //  stop command
                }
               
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
                int axisIndex = -1;

                if (axisName=="XY") 
                { 
                    axisName = "X";
                    axisIndex = GetAxisIndex(axisName);
                    trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, axisIndex, out isAxisEnabledD[axisIndex]);
                    isAxisEnabled[axisIndex] = (isAxisEnabledD[axisIndex] == 1);

                }

                axisIndex = GetAxisIndex(axisName);
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

                if (axisName == "X")// trio
                {
                   
                   
                    double distanceValue = double.Parse(distance);
                    // Prepare parameters for the MoveRel function
                    double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                    int baseAxis = axisIndex;                            // Base axis for movement
                    double velocityValue = double.Parse(velocity);
                    trioMotionControl.SetMotion(baseAxis, velocityValue);//can also set Accel, Decel, Jerk

                    if (moveType == "Jog")
                    {
                        //trioMotionControl.Base();
                        bool JogSuccessful = trioMotionControl.Reverse(baseAxis);
                        if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                        else { Debug.Print("JogSuccessful: 0"); }
                    }

                    if (moveType == "Relative Move")
                    {

                        // Call MoveRel
                        // Negate each element in the array
                        for (int i = 0; i < distances.Length; i++)
                        {
                            distances[i] = -distances[i];
                        }

                        bool success = trioMotionControl.MoveRel(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move on X axis completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move on X axis failed.");
                        }

                    }

                    if (moveType == "Absolute Move")
                    {
                        // Call MoveAbs
                        bool success = trioMotionControl.MoveAbs(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move on X axis completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move on X axis failed.");
                        }

                    }

                }

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

                        var command = stepperMotorControl.GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, positive: false);
                        stepperMotorControl.SendCommand(command);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number.");
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }



        private void ReverseButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (axisName == "XY")
            {
                axisName = "X"; //this button is for X
            }
            int axisIndex = GetAxisIndex(axisName);

            if (moveType == "Jog")
            {
                // Stop jogging
                if (axisName == "X") //X axis
                {

                    trioMotionControl.Cancel(0, axisIndex);//.0 cancels the current move on the base axis, 1 cancels the buffered moves on the base axis.

                }
                else if (axisName == "L") //L axis
                {
                    stepperMotorControl.SendCommand("M410"); //  stop command
                }

            }
        }

        private void HomeXButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("X");
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);

            if (isAxisEnabledD[AxisIndex] == 1.0)
            {
                trioMotionControl.Run("HOME_X", -1);//-1 means the next available process
            }
            else
            {
                MessageBox.Show("Enable the axis X first.");
            }
        }

        private void HomeYButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("Y");
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);

            if (isAxisEnabledD[AxisIndex] == 1.0)
            {
                trioMotionControl.Run("HOME_Y", -1);//-1 means the next available process
            }
            else
            {
                MessageBox.Show("Enable the axis Y first.");
            }
        }

        private void HomeZButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("Z");
            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);

            if (isAxisEnabledD[AxisIndex] == 1.0)
            {
                trioMotionControl.Run("HOME_Z", -1);//-1 means the next available process
            }
            else
            {
                MessageBox.Show("Enable the axis Z first.");
            }
        }

        private void HomeSAButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAxisEnabled[3])
            {
                MessageBox.Show("Enable the axis SA first.");
                return;
            }
            else
            {
                stepperMotorControl.SendCommand("G28 X");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                stepperMotorControl.SendCommand("G91\n G0 X7 F120");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                stepperMotorControl.SendCommand("G92 X0");
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
                stepperMotorControl.SendCommand("G28 Y");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                stepperMotorControl.SendCommand("G91\n G0 Y15 F120");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                stepperMotorControl.SendCommand("G92 Y0");
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
                stepperMotorControl.SendCommand("G92 Z0");
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
            stepperMotorControl.StopMonitoringPosition();
            trioMotionControl?.Dispose();
        }

        private void StopAllMotion_Click(object sender, RoutedEventArgs e)
        {
            trioMotionControl.RapidStop();//Stop X,Y and Z axis
            stepperMotorControl.SendCommand("M410"); //Quickstop
            
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
            handleMoveButtonsVisibility();
        }
        private void handleMoveButtonsVisibility()
        {
            // Hide all buttons by default
            UpButton.Visibility = Visibility.Collapsed;
            DownButton.Visibility = Visibility.Collapsed;
            ForwardButton.Visibility = Visibility.Collapsed;
            ReverseButton.Visibility = Visibility.Collapsed;
            UpLeftButton.Visibility = Visibility.Collapsed;
            UpRightButton.Visibility = Visibility.Collapsed;
            DownLeftButton.Visibility = Visibility.Collapsed;
            DownRightButton.Visibility = Visibility.Collapsed;

            if (AxisSelection.SelectedItem is ComboBoxItem axisItem)
            {
                string axis = axisItem.Content.ToString();

                switch (axis)
                {
                    case "XY":
                        // Show all movement buttons for XY
                        UpButton.Visibility = Visibility.Visible;
                        DownButton.Visibility = Visibility.Visible;
                        ForwardButton.Visibility = Visibility.Visible;
                        ReverseButton.Visibility = Visibility.Visible;
                        UpLeftButton.Visibility = Visibility.Visible;
                        UpRightButton.Visibility = Visibility.Visible;
                        DownLeftButton.Visibility = Visibility.Visible;
                        DownRightButton.Visibility = Visibility.Visible;
                        break;

                    case "Z":
                    case "SA":
                    case "SF":
                        // Show only up and down buttons for Z, SA, SF
                        UpButton.Visibility = Visibility.Visible;
                        DownButton.Visibility = Visibility.Visible;
                        break;

                    case "L":
                        // Show only forward and reverse buttons for L
                        ForwardButton.Visibility = Visibility.Visible;
                        ReverseButton.Visibility = Visibility.Visible;
                        break;
                }
            }
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

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AxisSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CameraSetting_Click(object sender, RoutedEventArgs e)
        {

            if (cameraSettingWindow == null || !cameraSettingWindow.IsVisible)
            {
                cameraSettingWindow = new CameraSetting();


                // close window
                cameraSettingWindow.Closed += (s, args) =>
                {
                    cameraSettingWindow = null;
                    Debug.Print("CameraSetting window closed.");
                };

                cameraSettingWindow.Show();
            }
            else
            {
                cameraSettingWindow.Focus();
            }
        }

        private void TrioControllerConnection_Click(object sender, RoutedEventArgs e)
        {
            if (trioControllerConnectionWindow == null || !trioControllerConnectionWindow.IsVisible)
            {
                trioControllerConnectionWindow = new TrioControllerConnection(trioMotionControl);


                // close window
                trioControllerConnectionWindow.Closed += (s, args) =>
                {
                    trioControllerConnectionWindow = null;
                    Debug.Print("trioControllerConnection window closed.");
                };

                trioControllerConnectionWindow.ShowDialog();
            }
            else
            {
                trioControllerConnectionWindow.Focus(); 
            }
        }

        private void BigTechControllerConnection_Click(object sender, RoutedEventArgs e)
        {
            if (bigTechControllerConnectionWindow == null || !bigTechControllerConnectionWindow.IsVisible)
            {
                bigTechControllerConnectionWindow = new BigTechControllerConnection(stepperMotorControl);



                bigTechControllerConnectionWindow.Closed += (s, args) =>
                {
                    bigTechControllerConnectionWindow = null;
                    Debug.Print("bigTechControllerConnection window closed.");
                };

                bigTechControllerConnectionWindow.ShowDialog();
            }
            else
            {
                bigTechControllerConnectionWindow.Focus(); // 如果窗口已打开，则将其置于前台
            }

        }

        // Update the UI with the current status
        private void UpdateStatusDisplay(string status)
        {
            AxisStatus.Text = status;

        }

        private void AxisConfiguration_Click(object sender, RoutedEventArgs e)
        {

        }



        private void ShowVR15button_Click(object sender, RoutedEventArgs e)
        {
            Double VRValue = 0;
            trioMotionControl.GetVr(15, out VRValue);
            VR15TextBox.Text = VRValue.ToString();
        }

        private void UpButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var velocity = VelocityInput.Text;
                var distance = DistanceInput.Text;
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                int axisIndex = -1;

                if (axisName == "XY")
                {
                    axisName = "Y"; //this button is for Y
                }
                axisIndex = GetAxisIndex(axisName);
                if (axisIndex == -1)
                {
                    MessageBox.Show("Invalid axis selected.");
                    return;
                }

                if (axisName == "Y" || axisName == "Z")
                {

                    trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, axisIndex, out isAxisEnabledD[axisIndex]);
                    isAxisEnabled[axisIndex] = (isAxisEnabledD[axisIndex] == 1);
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

                if (axisName == "Y" || axisName == "Z")// trio
                {
                   
                    double distanceValue = double.Parse(distance);
                    // Prepare parameters for the MoveRel function
                    double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                    int baseAxis = axisIndex;                            // Base axis for movement
                    double velocityValue = double.Parse(velocity);
                    trioMotionControl.SetMotion(baseAxis, velocityValue);//can also set Accel, Decel, Jerk

                    if (moveType == "Jog")
                    {
                        //trioMotionControl.Base();
                        bool JogSuccessful = trioMotionControl.Forward(baseAxis);
                        if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                        else { Debug.Print("JogSuccessful: 0"); }
                    }

                    if (moveType == "Relative Move")
                    {

                        // Call MoveRel
                        bool success = trioMotionControl.MoveRel(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move failed.");
                        }

                    }

                    if (moveType == "Absolute Move")
                    {
                        // Call MoveAbs
                        bool success = trioMotionControl.MoveAbs(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move failed.");
                        }

                    }


                }//if axis is Y or Z






                if (axisName == "SA" || axisName == "SF")
                {
                    if (moveType == "Jog")
                    {
                        double.TryParse(Max_Pos[axisIndex], out double MaxJogPos);
                        MaxJogPos = MaxJogPos - 0.1;
                        distance = MaxJogPos.ToString();
                    }
                    var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);
                    stepperMotorControl.SendCommand(command);
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

           

        }

        private void UpButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (axisName == "XY")
            {
                axisName = "Y"; //this button is for Y or Z
            }
            int axisIndex = GetAxisIndex(axisName);


            if (moveType == "Jog")
            {
                // Stop jogging
                if (axisName == "X") //X axis
                {

                    trioMotionControl.Cancel(0, axisIndex);//.0 cancels the current move on the base axis, 1 cancels the buffered moves on the base axis.

                }
                else if (axisName == "L") //L axis
                {
                    stepperMotorControl.SendCommand("M410"); //  stop command
                }

            }
        }

        private void DownButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var velocity = VelocityInput.Text;
                var distance = DistanceInput.Text;
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

                int axisIndex = -1;
                if (axisName == "XY")
                {
                    axisName = "Y";
                   
                }
                axisIndex = GetAxisIndex(axisName);
                if (axisIndex == -1)
                {
                    MessageBox.Show("Invalid axis selected.");
                    return;
                }


                if (axisName=="Y" || axisName=="Z")
                {
                    
                    trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, axisIndex, out isAxisEnabledD[axisIndex]);
                    isAxisEnabled[axisIndex] = (isAxisEnabledD[axisIndex] == 1);
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

                if (axisName == "Y" || axisName == "Z")// trio
                {


                    double distanceValue = double.Parse(distance);
                    // Prepare parameters for the MoveRel function
                    double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                    int baseAxis = axisIndex;                            // Base axis for movement
                    double velocityValue = double.Parse(velocity);
                    trioMotionControl.SetMotion(baseAxis, velocityValue);//can also set Accel, Decel, Jerk

                    if (moveType == "Jog")
                    {
                        //trioMotionControl.Base();
                        bool JogSuccessful = trioMotionControl.Reverse(baseAxis);
                        if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                        else { Debug.Print("JogSuccessful: 0"); }
                    }

                    if (moveType == "Relative Move")
                    {

                        // Call MoveRel
                        // Negate each element in the array
                        for (int i = 0; i < distances.Length; i++)
                        {
                            distances[i] = -distances[i];
                        }

                        bool success = trioMotionControl.MoveRel(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move  completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move  failed.");
                        }

                    }

                    if (moveType == "Absolute Move")
                    {
                        // Call MoveAbs
                        bool success = trioMotionControl.MoveAbs(distances, baseAxis);

                        if (success)
                        {
                            Console.WriteLine("Relative move completed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Relative move failed.");
                        }

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
                    var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: false);
                    stepperMotorControl.SendCommand(command);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }



        }

        private void DownButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            var axisName = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (axisName == "XY")
            {
                axisName = "Y"; //this button is for Y
            }
            int axisIndex = GetAxisIndex(axisName);

            if (moveType == "Jog")
            {
                // Stop jogging
                if (axisName == "Y") //Y axis
                {

                    trioMotionControl.Cancel(0, axisIndex);//.0 cancels the current move on the base axis, 1 cancels the buffered moves on the base axis.

                }
                else if (axisName == "L") //L axis
                {
                    stepperMotorControl.SendCommand("M410"); //  stop command
                }

            }


        }

        private void UpLeftButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UpLeftButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void UpRightButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void UpRightButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DownLeftButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DownLeftButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DownRightButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DownRightButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

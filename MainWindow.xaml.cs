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
using TrioMotion.TrioPC_NET;
using System.Net;
using System.IO;
using Windows.UI.Core;
using static System.Net.WebRequestMethods;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;


namespace TipShaping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Define the field
        private bool[] isAxisEnabled = { false, false, false, false, false, false, false };
        double[] isAxisEnabledD = { 0, 0, 0, 0, 0, 0,0 };
        double[] isDriveEnabledD = { 0, 0, 0, 0, 0, 0,0 };
        string[] Max_Pos = { "105", "105", "35", "4.5", "29", "360000", "360000" }; //forward travel limit SA,SF 13mm 30mm
        string[] Min_Pos = { "-105", "-105", "-35", "-1.5", "0", "-360000", "-360000" }; //reverse travel limit
                                                                             //private string SAlastPosition = string.Empty;
                                                                             //private string SFlastPosition = string.Empty;
                                                                             //private string LlastPosition = string.Empty;

        bool trioAllIdle = true; // Assume all axes are idle initially
        bool bigTechAllIdle = true; // Assume all axes are idle initially
        bool allIdle = true;
        private CameraSetting cameraSettingWindow;
        private BigTechControllerConnection bigTechControllerConnectionWindow;
        private TrioControllerConnection trioControllerConnectionWindow;
        private StepperMotorControl stepperMotorControl;
        private TrioMotionControl trioMotionControl;
        private DispatcherTimer allMovingStatusUpdateTimer;

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
            trioMotionControl.TrioPositionUpdated += TrioUpdatePositions;
            trioMotionControl.TrioEnableButtonContentUpdated += InitialEnableButtons;

            // Initialize the timer
            allMovingStatusUpdateTimer = new DispatcherTimer();
            allMovingStatusUpdateTimer.Interval = TimeSpan.FromMilliseconds(100); // Set interval to 100 ms
            allMovingStatusUpdateTimer.Tick += UpdateAllAxisStatus; // Attach the Tick event handler
            allMovingStatusUpdateTimer.Start(); // Start the timer


        }

        private void UpdateAllAxisStatus(object sender, EventArgs e)
        {
            allIdle= bigTechAllIdle && trioAllIdle;
            if (!allIdle)
            {
                AxisStatus.Text = "Moving";
            }
            else
            { AxisStatus.Text = "Idle"; }
            allIdle = true;//rest
            bigTechAllIdle = true;//rest
            trioAllIdle = true;//rest
        }

        private void InitialEnableButtons(double[] enableValue)
        {
            EnableXButton.Content = enableValue[1] == 0 ? "Enable X" : "Disable X";
            EnableYButton.Content = enableValue[0] == 0 ? "Enable Y" : "Disable Y";
            EnableZButton.Content = enableValue[2] == 0 ? "Enable Z" : "Disable Z";
        }



        private void TrioUpdatePositions(double x, double y, double z)
        {
            double XMPos = y;
            double YMPos = x;
            double ZMPos = z;

            try
            {
                // Fetch the measured positions
               
                XMPosTextBox.Text = XMPos.ToString("F3");
                YMPosTextBox.Text = YMPos.ToString("F3");
                ZMPosTextBox.Text = ZMPos.ToString("F3");

                if (AxisSelection.SelectedItem != null)
                {
                    string selectedAxis = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

                    if (selectedAxis == "XY") // 0: Y, 1: X
                    {
                        if (moveType =="Absolute Move")
                        {
                            //ReverseRestTravelTextBlock.Text = $"{Min_Pos[1]:F3}";
                            ForwardRestTravelTextBlock.Text = $"({Min_Pos[1]:F3}, {Max_Pos[1]:F3})";
                            UpRestTravelTextBlock.Text = $"({Min_Pos[0]:F3}, {Max_Pos[0]:F3})";
                            //DownRestTravelTextBlock.Text = $"{Min_Pos[0]:F3}";

                        }
                        else
                        {
                            ReverseRestTravelTextBlock.Text = Math.Abs(double.Parse(Min_Pos[1]) - XMPos).ToString("F3");
                            ForwardRestTravelTextBlock.Text = Math.Abs(double.Parse(Max_Pos[1]) - XMPos).ToString("F3");
                            UpRestTravelTextBlock.Text = Math.Abs(double.Parse(Max_Pos[0]) - YMPos).ToString("F3");
                            DownRestTravelTextBlock.Text = Math.Abs(double.Parse(Min_Pos[0]) - YMPos).ToString("F3");

                        }

                    }
                    else if (selectedAxis == "Z")
                    {
                        if (moveType == "Absolute Move")
                        {
                            UpRestTravelTextBlock.Text = $"({Min_Pos[2]:F3}, {Max_Pos[2]:F3})";
                            //DownRestTravelTextBlock.Text = $"{Min_Pos[2]:F3}";
                        }
                        else
                        {                          
                            UpRestTravelTextBlock.Text = Math.Abs(double.Parse(Max_Pos[2]) - YMPos).ToString("F3");
                            DownRestTravelTextBlock.Text = Math.Abs(double.Parse(Min_Pos[2]) - YMPos).ToString("F3");
                        }
                                              

                    }
                }

                trioAllIdle = true; // Assume all axes are idle initially
                double[] IfIdle = new double[3];
                
                for (int i = 0;i<3;i++)
                {
                    IfIdle[i] = 1;
                   
                    trioMotionControl.GetAxisParameter(AxisParameter.IDLE, i, out IfIdle[i]);
                    if (IfIdle[i] == 0)
                    {
                        trioAllIdle = false;
                    }
                    //Debug.Print($"IfIdle axis {i}:{IfIdle[i]}");
                    // Update AxisStatus.Text based on IfIdle
                }

                // Update AxisStatus based on the status of all axes
               // AxisStatus.Text = allIdle ? "Idle" : "Moving";
                //Debug.Print($"Overall Status: {AxisStatus.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating positions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void StepperMotorOnPositionUpdated(float x, float y, float z, float e)
        {
            // Update the UI
            SAMPosTextBox.Text = $"{x:F3}";
            SFMPosTextBox.Text = $"{y:F3}";
            double zDeg = z * 360;
            SVMPosTextBox.Text = $"{zDeg:F3}";

            double eDeg = e * 360;
            LMPosTextBox.Text = $"{eDeg:F3}";



            if (AxisSelection.SelectedItem != null)
            {
                string selectedAxis = (AxisSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                var moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (selectedAxis == "SA")
                { 
                    if(moveType == "Absolute Move")
                    {
                        UpRestTravelTextBlock.Text = $"({Min_Pos[3]:F3}, {Max_Pos[3]:F3})";
                        //DownRestTravelTextBlock.Text = $"{Min_Pos[3]:F3}";
                    }
                    else
                    {
                        UpRestTravelTextBlock.Text = Math.Abs(float.Parse(Max_Pos[3]) - x).ToString("F3");
                        DownRestTravelTextBlock.Text = Math.Abs(float.Parse(Min_Pos[3]) - x).ToString("F3");
                    }
                    
                   
                }
                else if (selectedAxis == "SF")
                {
                    if (moveType == "Absolute Move")
                    {
                        UpRestTravelTextBlock.Text = $"({Min_Pos[4]:F3}, {Max_Pos[4]:F3})";
                        //DownRestTravelTextBlock.Text = $"{Min_Pos[4]:F3}";
                    }
                    else
                    {
                        UpRestTravelTextBlock.Text = Math.Abs(float.Parse(Max_Pos[4]) - y).ToString("F3");
                        DownRestTravelTextBlock.Text = Math.Abs(float.Parse(Min_Pos[4]) - y).ToString("F3");
                    }

                   
                } 
                else if (selectedAxis == "L")
                {
                    //if (moveType == "Absolute Move")
                    //{
                    //    ForwardRestTravelTextBlock.Text = $"{Max_Pos[5]:F3}";
                    //    ReverseRestTravelTextBlock.Text = $"{Min_Pos[5]:F3}";
                    //}
                    //else
                    //{
                    //    ForwardRestTravelTextBlock.Text = Math.Abs(float.Parse(Max_Pos[5]) - z).ToString("F3");
                    //    ReverseRestTravelTextBlock.Text = Math.Abs(float.Parse(Min_Pos[5]) - z).ToString("F3");
                    //}

                    //ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                    //ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
                    //UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                    //DownRestTravelTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void StepperMotorOnMovingStatusUpdated(bool Moving)
        {
            // Update the UI
           bigTechAllIdle = !Moving;
            
            //if (!allIdle)
            //{
            //    AxisStatus.Text = "Moving";
            //}
            //else
            //{ AxisStatus.Text = "Idle"; }

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
                   
                    // Add code to enable X
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                    MessageBox.Show("X Enabled");
                }
                else
                {
                   
                    // Add code to disable X
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                    MessageBox.Show("X Disabled");
                }

                //update one more time after action
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);
                // Update the button content
                EnableXButton.Content = isAxisEnabled[AxisIndex] ? "Disable X" : "Enable X";
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
                    
                    // Add code to enable Y
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                    MessageBox.Show("Y Enabled");
                }
                else
                {
                    
                    // Add code to disable Y
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                    MessageBox.Show("Y Disabled");
                }

                //update one more time after action
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);
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
                    
                    // Add code to enable Z
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 1);
                    MessageBox.Show("Z Enabled");
                }
                else
                {
                    
                    // Add code to disable Z
                    trioMotionControl.SetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, 0);
                    MessageBox.Show("Z Disabled");
                }


                //update one more time after action
                trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.AXIS_ENABLE, AxisIndex, out isAxisEnabledD[AxisIndex]);
                isAxisEnabled[AxisIndex] = (isAxisEnabledD[AxisIndex] == 1);
                // Update the button content
                EnableZButton.Content = isAxisEnabled[AxisIndex] ? "Disable Z" : "Enable Z";
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
                    string enableCommand = "M17 E"; // M17 is the Marlin G-code to engage stepper motors
                    stepperMotorControl.SendCommand(enableCommand);
                    MessageBox.Show("L Enabled");
                }
                else
                {
                    // Send G-code to disable the corresponding stepper motor
                    string disableCommand = "M18 E"; // M18 disengages the stepper motor
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
                "SV" => 5,
                "L" => 6,
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
                    double IfIDLE = 0;
                    trioMotionControl.GetAxisParameter(AxisParameter.IDLE, axisIndex, out IfIDLE);
                    if(IfIDLE !=0)
                    {
                        int baseAxis = axisIndex;                            // Base axis for movement
                        double velocityValue = double.Parse(velocity);
                        //Debug.Print($"velocity:{velocity}");
                        double Accel = 1000;
                        double Decel = 1000;
                        double Jerk = 2000;

                        trioMotionControl.SetMotion(baseAxis, velocityValue, Accel, Decel, Jerk);//can also set Accel, Decel, Jerk
                        Debug.Print("Set Motion successfully.");

                        if (moveType == "Jog")
                        {
                            //trioMotionControl.Base();

                            bool JogSuccessful = trioMotionControl.Forward(baseAxis);
                            if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                            else { Debug.Print("JogSuccessful: 0"); }
                        }
                        else
                        {
                            double distanceValue = double.Parse(distance);
                            // Prepare parameters for the MoveRel function
                            double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                            double AxisPos;

                            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.MPOS, axisIndex, out AxisPos);

                            if (moveType == "Relative Move")
                            {
                                double targetPos = distances[0] + AxisPos;
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
                                {

                                    try
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
                                    catch (Exception ex) { Debug.Print(axisName + ": " + ex.ToString()); }

                                }
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }


                            }

                            if (moveType == "Absolute Move")
                            {
                                double targetPos = distances[0];
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }

                            }

                        }

                    }
                    else
                    {
                        MessageBox.Show($"Axis {axisIndex} is still Moving. Wait for idle.");
                    }

                }


                if (axisName == "L" || axisName == "SV")
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
                    
                    trioMotionControl.Cancel(2, axisIndex);//.2 cancels all move on the base axis, 1 cancels the buffered moves on the base axis.

                }
                else if (axisName == "L" || axisName == "SV") //L axis
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
                    double IfIDLE = 0;
                    trioMotionControl.GetAxisParameter(AxisParameter.IDLE, axisIndex, out IfIDLE);
                    if (IfIDLE != 0)
                    {
                        int baseAxis = axisIndex;                            // Base axis for movement
                        double velocityValue = double.Parse(velocity);
                        double Accel = 1000;
                        double Decel = 1000;
                        double Jerk = 2000;
                        trioMotionControl.SetMotion(baseAxis, velocityValue, Accel, Decel, Jerk);//can also set Accel, Decel, Jerk

                        if (moveType == "Jog")
                        {
                            //trioMotionControl.Base();
                            bool JogSuccessful = trioMotionControl.Reverse(baseAxis);
                            if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                            else { Debug.Print("JogSuccessful: 0"); }
                        }
                        else
                        {

                            double distanceValue = double.Parse(distance);
                            // Prepare parameters for the MoveRel function
                            double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                            double AxisPos;
                            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.MPOS, axisIndex, out AxisPos);


                            if (moveType == "Relative Move")
                            {
                                for (int i = 0; i < distances.Length; i++)
                                {
                                    distances[i] = -distances[i];
                                }

                                double targetPos = distances[0] + AxisPos;
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
                                {
                                    // Call MoveRel
                                    // Negate each element in the array


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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }


                            }

                            if (moveType == "Absolute Move")
                            {
                                double targetPos = distances[0];
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }

                            }


                        }
                    }
                    else
                    {
                        MessageBox.Show($"Axis {axisIndex} is still Moving. Wait for idle.");
                    }


                   
                    

                   

                }

                if (axisName == "L" || axisName == "SV")
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

                    trioMotionControl.Cancel(2, axisIndex);

                }
                else if (axisName == "L" || axisName == "SV") //L axis
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
                stepperMotorControl.SendCommand("G91\n G0 X-5 F120");
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
                stepperMotorControl.SendCommand("G91\n G0 Y0.5 F120");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }
                stepperMotorControl.SendCommand("G92 Y0");
            }
        }

        private void HomeLButton_Click(object sender, RoutedEventArgs e)
        {
            

            if (!isAxisEnabled[6]) //L:6
            {
                MessageBox.Show("Enable the axis L first.");
                return;
            }
            else
            {
                stepperMotorControl.SendCommand("G92 E0");
            }

        }



        // Window closing event to stop monitoring
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stepperMotorControl.StopMonitoringPosition();
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 0, 0);//Y
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 1, 0);//X
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 2, 0);//Z
            trioMotionControl?.Dispose();
        }

        private void StopAllMotion_Click(object sender, RoutedEventArgs e)
        {
            trioMotionControl.RapidStop();//Stop X,Y and Z axis
            if (stepperMotorControl.IsSerialPortOpen())
            { stepperMotorControl.SendCommand("M410"); }//Quickstop
            
        }

        private void AxisSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AxisSelection.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content.ToString();
                DistanceUnit.Text = (content == "L" || content == "SV") ? "deg" : "mm";

                VelocityTextBlock.Text = (content == "L" || content == "SV") ? "Velocity(rev/s):" : "Velocity(mm/s):";
            }

            HandleRotationButtonsVisibility();
            handleMoveButtonsAndRestTravelTextBlockVisibility(); 

        }
        private void handleMoveButtonsAndRestTravelTextBlockVisibility()
        {
            string moveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();
            // Hide all buttons by default
            UpButton.Visibility = Visibility.Collapsed;
            DownButton.Visibility = Visibility.Collapsed;
            ForwardButton.Visibility = Visibility.Collapsed;
            ReverseButton.Visibility = Visibility.Collapsed;
            UpLeftButton.Visibility = Visibility.Collapsed;
            UpRightButton.Visibility = Visibility.Collapsed;
            DownLeftButton.Visibility = Visibility.Collapsed;
            DownRightButton.Visibility = Visibility.Collapsed;

            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;

            if (AxisSelection.SelectedItem is ComboBoxItem axisItem)
            {
                string axis = axisItem.Content.ToString();

                switch (axis)
                {
                    case "XY":
                        // Show all movement buttons for XY


                        if (MoveTypeSelection.SelectedItem == null)
                        {
                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Visible;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Visible;
                            UpLeftButton.Visibility = Visibility.Visible;
                            UpRightButton.Visibility = Visibility.Visible;
                            DownLeftButton.Visibility = Visibility.Visible;
                            DownRightButton.Visibility = Visibility.Visible;

                            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;

                        }
                        if (moveType == "Relative Move" || moveType == "Jog")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Visible;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Visible;
                            UpRestTravelTextBlock.Visibility = Visibility.Visible;
                            DownRestTravelTextBlock.Visibility = Visibility.Visible;

                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Visible;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Visible;
                            UpLeftButton.Visibility = Visibility.Visible;
                            UpRightButton.Visibility = Visibility.Visible;
                            DownLeftButton.Visibility = Visibility.Visible;
                            DownRightButton.Visibility = Visibility.Visible;

                        }

                        if (moveType == "Absolute Move")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Visible;
                            UpRestTravelTextBlock.Visibility = Visibility.Visible;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Collapsed;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Collapsed;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;
                        }


                        break;

                    case "Z":
                    case "SA":
                    case "SF":

                        // Show only up and down buttons for Z, SA, SF
                        //UpButton.Visibility = Visibility.Visible;
                        //DownButton.Visibility = Visibility.Visible;

                        if (MoveTypeSelection.SelectedItem == null)
                        {
                            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Visible;
                            ForwardButton.Visibility = Visibility.Collapsed;
                            ReverseButton.Visibility = Visibility.Collapsed;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;


                        }
                        if (moveType == "Relative Move" || moveType == "Jog")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            UpRestTravelTextBlock.Visibility = Visibility.Visible;
                            DownRestTravelTextBlock.Visibility = Visibility.Visible;

                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Visible;
                            ForwardButton.Visibility = Visibility.Collapsed;
                            ReverseButton.Visibility = Visibility.Collapsed;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;

                        }

                        if (moveType == "Absolute Move")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            UpRestTravelTextBlock.Visibility = Visibility.Visible;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Visible;
                            DownButton.Visibility = Visibility.Collapsed;
                            ForwardButton.Visibility = Visibility.Collapsed;
                            ReverseButton.Visibility = Visibility.Collapsed;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;
                        }


                        break;

                    case "SV":
                    case "L":

                        if (MoveTypeSelection.SelectedItem == null)
                        {
                            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Collapsed;
                            DownButton.Visibility = Visibility.Collapsed;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Visible;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;
                        }
                        if (moveType == "Relative Move" || moveType == "Jog")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Collapsed;
                            DownButton.Visibility = Visibility.Collapsed;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Visible;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;

                        }
                        if (moveType == "Absolute Move")
                        {
                            ReverseRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            ForwardRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            UpRestTravelTextBlock.Visibility = Visibility.Collapsed;
                            DownRestTravelTextBlock.Visibility = Visibility.Collapsed;

                            UpButton.Visibility = Visibility.Collapsed;
                            DownButton.Visibility = Visibility.Collapsed;
                            ForwardButton.Visibility = Visibility.Visible;
                            ReverseButton.Visibility = Visibility.Collapsed;
                            UpLeftButton.Visibility = Visibility.Collapsed;
                            UpRightButton.Visibility = Visibility.Collapsed;
                            DownLeftButton.Visibility = Visibility.Collapsed;
                            DownRightButton.Visibility = Visibility.Collapsed;
                        }




                        break;
                }
            }
        }


        private void RotationButton4L_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button button)
            {
                DistanceInput.Text = button.Content.ToString();
            }

        }

        private void MoveTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleRotationButtonsVisibility();
         
            handleMoveButtonsAndRestTravelTextBlockVisibility();
        }


        private void HandleRotationButtonsVisibility()
        {
            // Set the visibility of the L panel, when the axis is L and the move type is not "Jog"
            SetPanelVisibility("L", RotationButtonsPanel4L, moveTypeNotJog: true);

            // Set the visibility of the SV panel, only when the axis is SV and the move type is "Absolute Move"
            SetPanelVisibility("SV", RotationButtonsPanel4SV, moveType: "Absolute Move");
        }

        void SetPanelVisibility(string axisType, UIElement panel, string moveType = null, bool moveTypeNotJog = false)
        {
            bool isAxisSelected = AxisSelection.SelectedItem is ComboBoxItem axisItem && axisItem.Content.ToString() == axisType;

            // Get the selected move type only once
            string selectedMoveType = (MoveTypeSelection.SelectedItem as ComboBoxItem)?.Content.ToString();

            bool isMoveTypeCorrect = moveTypeNotJog
                ? selectedMoveType != "Jog"  // Check if the move type is not "Jog"
                : string.IsNullOrEmpty(moveType) || selectedMoveType == moveType;  // Check if the move type matches or if moveType is not provided

            // Set the visibility of the panel based on the axis type and move type
            panel.Visibility = isAxisSelected && isMoveTypeCorrect ? Visibility.Visible : Visibility.Collapsed;
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
                    double IfIDLE = 0;
                    trioMotionControl.GetAxisParameter(AxisParameter.IDLE, axisIndex, out IfIDLE);
                    if (IfIDLE != 0)
                    {
                        int baseAxis = axisIndex;                            // Base axis for movement
                        double velocityValue = double.Parse(velocity);
                        double Accel = 1000;
                        double Decel = 1000;
                        double Jerk = 2000;
                        trioMotionControl.SetMotion(baseAxis, velocityValue, Accel, Decel, Jerk);//can also set Accel, Decel, Jerk

                        if (moveType == "Jog")
                        {
                            //trioMotionControl.Base();
                            bool JogSuccessful = trioMotionControl.Forward(baseAxis);
                            if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                            else { Debug.Print("JogSuccessful: 0"); }
                        }
                        else
                        {
                            double distanceValue = double.Parse(distance);
                            // Prepare parameters for the MoveRel function
                            double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                            double AxisPos;
                            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.MPOS, axisIndex, out AxisPos);



                            if (moveType == "Relative Move")
                            {
                                double targetPos = distances[0] + AxisPos;
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }


                            }

                            if (moveType == "Absolute Move")
                            {
                                double targetPos = distances[0];
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }

                            }

                        }

                    }
                    else
                    {
                        MessageBox.Show($"Axis {axisIndex} is still Moving. Wait for idle.");
                    }

                    

                }//if axis is Y or Z



                if (axisName == "SA" || axisName == "SF")
                {
                    if (moveType == "Jog")
                    {
                        double.TryParse(Max_Pos[axisIndex], out double MaxJogPos);
                        //MaxJogPos = MaxJogPos-0.1;
                        distance = MaxJogPos.ToString();

                        var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);
                        stepperMotorControl.SendCommand(command);
                    }

                    if (moveType == "Absolute Move")
                    {
                        if (double.Parse(distance) > double.Parse(Min_Pos[axisIndex]) && double.Parse(distance) < double.Parse(Max_Pos[axisIndex]))
                        {
                            var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);
                            stepperMotorControl.SendCommand(command);
                        }
                        else
                        {
                            MessageBox.Show($"Error: Target distance is out of limit!");
                        }

                    }
                    if (moveType == "Relative Move")
                    {
                        if (axisName == "SA")
                        {
                            if ((double.Parse(SAMPosTextBox.Text) + double.Parse(distance)) > double.Parse(Min_Pos[axisIndex]) && (double.Parse(SAMPosTextBox.Text) + double.Parse(distance)) < double.Parse(Max_Pos[axisIndex]))
                            {
                                var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);
                                stepperMotorControl.SendCommand(command);
                            }
                            else
                            {
                                MessageBox.Show($"Error: Target distance is out of limit!");
                            }
                        }
                        
                        if (axisName == "SF")
                        {
                            if ((double.Parse(SFMPosTextBox.Text) + double.Parse(distance)) > double.Parse(Min_Pos[axisIndex]) && (double.Parse(SFMPosTextBox.Text) + double.Parse(distance)) < double.Parse(Max_Pos[axisIndex]))
                            {
                                var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);
                                stepperMotorControl.SendCommand(command);
                            }
                            else
                            {
                                MessageBox.Show($"Error: Target distance is out of limit!");
                            }
                        }

                       

                    }




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
                if (axisName == "Y" || axisName == "Z") //X axis
                {

                    trioMotionControl.Cancel(2, axisIndex);

                }
                else if (axisName == "SF" || axisName == "SA") 
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
                    double IfIDLE = 0;
                    trioMotionControl.GetAxisParameter(AxisParameter.IDLE, axisIndex, out IfIDLE);
                    if (IfIDLE != 0)
                    {
                        int baseAxis = axisIndex;                            // Base axis for movement
                        double velocityValue = double.Parse(velocity);
                        double Accel = 1000;
                        double Decel = 1000;
                        double Jerk = 2000;
                        trioMotionControl.SetMotion(baseAxis, velocityValue, Accel, Decel, Jerk);//can also set Accel, Decel, Jerk

                        if (moveType == "Jog")
                        {
                            //trioMotionControl.Base();
                            bool JogSuccessful = trioMotionControl.Reverse(baseAxis);
                            if (JogSuccessful) { Debug.Print("JogSuccessful: 1"); }
                            else { Debug.Print("JogSuccessful: 0"); }
                        }
                        else
                        {
                            double distanceValue = double.Parse(distance);
                            // Prepare parameters for the MoveRel function
                            double[] distances = new double[1] { distanceValue }; // Distance array for relative move
                            double AxisPos;
                            trioMotionControl.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.MPOS, axisIndex, out AxisPos);


                            if (moveType == "Relative Move")
                            {

                                // Call MoveRel
                                // Negate each element in the array
                                for (int i = 0; i < distances.Length; i++)
                                {
                                    distances[i] = -distances[i];
                                }

                                double targetPos = distances[0] + AxisPos;
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
                                {
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }


                            }

                            if (moveType == "Absolute Move")
                            {
                                double targetPos = distances[0];
                                if (targetPos > double.Parse(Min_Pos[axisIndex]) && targetPos < double.Parse(Max_Pos[axisIndex]))
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
                                else
                                {
                                    MessageBox.Show("Target position is out of travel limit!");
                                    return;
                                }


                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Axis {axisIndex} is still Moving. Wait for idle.");
                    }

                    

                }



                if (axisName == "SA" || axisName == "SF")
                { //add code to check that 
                    if (moveType == "Jog")
                    {
                        double.TryParse(Min_Pos[axisIndex], out double MinJogPos);
                        // MinJogPos = MinJogPos;
                        distance = MinJogPos.ToString();

                        var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: true);//use true
                        stepperMotorControl.SendCommand(command);
                    }
                   

                    if (moveType == "Absolute Move")
                    {
                        if (double.Parse(distance) > double.Parse(Min_Pos[axisIndex]) && double.Parse(distance) < double.Parse(Max_Pos[axisIndex]))
                        {
                            var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: false);
                            stepperMotorControl.SendCommand(command);
                        }
                        else
                        {
                            MessageBox.Show($"Error: Target distance is out of limit!");
                        }

                    }
                    if (moveType == "Relative Move")
                    {
                        if (axisName == "SA")
                        {
                            if ((double.Parse(SAMPosTextBox.Text) - double.Parse(distance)) > double.Parse(Min_Pos[axisIndex]) && (double.Parse(SAMPosTextBox.Text) - double.Parse(distance)) < double.Parse(Max_Pos[axisIndex]))
                            {
                                var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: false);
                                stepperMotorControl.SendCommand(command);
                            }
                            else
                            {
                                MessageBox.Show($"Error: Target distance is out of limit!");
                            }
                        }

                        if (axisName == "SF")
                        {
                            if ((double.Parse(SFMPosTextBox.Text) - double.Parse(distance)) > double.Parse(Min_Pos[axisIndex]) && (double.Parse(SFMPosTextBox.Text) - double.Parse(distance)) < double.Parse(Max_Pos[axisIndex]))
                            {
                                var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, positive: false);
                                stepperMotorControl.SendCommand(command);
                            }
                            else
                            {
                                MessageBox.Show($"Error: Target distance is out of limit!");
                            }
                        }



                    }

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
                if (axisName == "Y" || axisName == "Z") //Y axis
                {

                    trioMotionControl.Cancel(2, axisIndex);

                }
                else if (axisName == "SF" || axisName == "SA")
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

        private void DisconnectTrioButton_Click(object sender, RoutedEventArgs e)
        {
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 0, 0);
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 1, 0);
            trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 2, 0);
            trioMotionControl.DisconnectToController("127.0.0.1");
            trioMotionControl?.Dispose();
            Debug.Print("Trio Controller is disconnected.");
        }

        private void HomeSVButton_Click(object sender, RoutedEventArgs e)
        {
            int AxisIndex = GetAxisIndex("SV");
            if (!isAxisEnabled[AxisIndex])//SV:5
            {
                MessageBox.Show("Enable the axis SV first.");
                return;
            }
            else
            {
                stepperMotorControl.SendCommand("G28 Z");
                while (AxisStatus.Text == "Moving")
                {
                    //wait
                }

                stepperMotorControl.SendCommand("G92 Z0");
            }
        }

        private void EnableSVButton_Click(object sender, RoutedEventArgs e)
        {

            if (stepperMotorControl.serialPort.IsOpen)
            {
                int AxisIndex = GetAxisIndex("SV");
                if (!isAxisEnabled[AxisIndex])
                {
                    // Send G-code to enable the corresponding stepper motor 
                    string enableCommand = "M17 Z"; // M17 is the Marlin G-code to engage stepper motors
                    stepperMotorControl.SendCommand(enableCommand);
                    MessageBox.Show("SV Enabled");
                }
                else
                {
                    // Send G-code to disable the corresponding stepper motor
                    string disableCommand = "M18 Z"; // M18 disengages the stepper motor
                    stepperMotorControl.SendCommand(disableCommand);
                    MessageBox.Show("SV Disabled");
                }
                isAxisEnabled[AxisIndex] = !isAxisEnabled[AxisIndex];
                // Update the button content
                EnableSVButton.Content = isAxisEnabled[AxisIndex] ? "Disable SV" : "Enable SV";
            }
            else
            { MessageBox.Show("BigTech Controller is not connected!"); }
        }

        private void RotationButton4SV_Click(object sender, RoutedEventArgs e)
        {
            // Get the clicked button
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                
                foreach (Button btn in RotationButtonsPanel4SV.Children)
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5")); // Reset background color to light gray
                }

                // Set background color to green only for the clicked button
                clickedButton.Background = Brushes.Green;

                // Get the button's name, e.g., "RotationSV1", "RotationSV2", etc.
                string buttonName = clickedButton.Name;

                // Extract the numeric part from the button's name
                string numberPart = new string(buttonName.Where(char.IsDigit).ToArray());

                if (int.TryParse(numberPart, out int buttonNumber))
                {
                    double result = 360.0 / 27 * (buttonNumber - 1);

                    // Set the DistanceInput.Text based on the result
                    DistanceInput.Text = result.ToString("F3");
                }
                else
                {
                    // Handle error if no numeric value is found
                    DistanceInput.Text = "Error";
                }
            }
        }

    }
}

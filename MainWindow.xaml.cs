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
        
        // Define the field
        private bool[] isAxisEnabled = { false, false, false, false, false, false };
        string[] Max_Pos = {"100","100","50","6", "14", "36000" }; //forward travel limit SA,SF 13mm 30mm
        string[] Min_Pos = {"-100", "-100", "-50","-6", "-14", "-36000" }; //reverse travel limit
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
                    var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, forward: true);
                    stepperMotorControl.SendCommand(command);
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

                            var command = stepperMotorControl.GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, forward: true);
                            stepperMotorControl.SendCommand(command);
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
                stepperMotorControl.SendCommand("M410"); //  stop command
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
                    var command = stepperMotorControl.GenerateGCode(axisName, velocity, distance, moveType, forward: false);
                    stepperMotorControl.SendCommand(command);
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

                            var command = stepperMotorControl.GenerateGCode(axisName, velocity, distanceRev.ToString(), moveType, forward: false);
                            stepperMotorControl.SendCommand(command);
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
                stepperMotorControl.SendCommand("M410"); // Replace with your controller's specific stop command

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
        }

        private void StopAllMotion_Click(object sender, RoutedEventArgs e)
        {

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

          
                // 窗口关闭后清理资源
                cameraSettingWindow.Closed += (s, args) =>
                {
                    cameraSettingWindow = null;
                    Debug.Print("CameraSetting window closed.");
                };

                cameraSettingWindow.Show();
            }
            else
            {
                cameraSettingWindow.Focus(); // 如果窗口已打开，则将其置于前台
            }
        }

        private void TrioControllerConnection_Click(object sender, RoutedEventArgs e)
        {
            if (trioControllerConnectionWindow == null || !trioControllerConnectionWindow.IsVisible)
            {
                trioControllerConnectionWindow = new TrioControllerConnection(trioMotionControl);


                // 窗口关闭后清理资源
                trioControllerConnectionWindow.Closed += (s, args) =>
                {
                    trioControllerConnectionWindow = null;
                    Debug.Print("trioControllerConnection window closed.");
                };

                trioControllerConnectionWindow.ShowDialog();
            }
            else
            {
                trioControllerConnectionWindow.Focus(); // 如果窗口已打开，则将其置于前台
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
    }
}

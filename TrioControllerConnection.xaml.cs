using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
using TrioMotion.TrioPC_NET;

namespace TipShaping
{
    /// <summary>
    /// Interaction logic for TrioControllerConnection.xaml
    /// </summary>
    public partial class TrioControllerConnection : Window
    {
        TrioMotionControl trioMotionControl;

        public TrioControllerConnection(TrioMotionControl trioMotion)
        {
            trioMotionControl = new TrioMotionControl();
            InitializeComponent();

            trioMotionControl = trioMotion;

        }


        private void ConnectControllerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(IPAddress.Text))
            {
                try
                {
                    // Attempt to open a connection
                    var connectionResult = trioMotionControl.IsControllerConnected();
                    if (connectionResult != true)
                    {
                        // Connect to the controller
                        trioMotionControl.ConnectToController(IPAddress.Text);
                        Debug.Print("Connected to Trio Controller");
                        ConnectControllerButton.Content = "Disconnect";
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE,0,1);
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE,1,1);
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE,2,1);
                        trioMotionControl.Run("STARTUP", -1);//-1 means the next available process
                        trioMotionControl.IndicateAxisEnable();//indicate the Enable XYZ buttons.
                        Debug.Print("Drive enabled");
                    }
                    else
                    {
                        // Disconnect from the controller
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 0, 0);
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 1, 0);
                        trioMotionControl.SetAxisParameter(AxisParameter.DRIVE_ENABLE, 2, 0);
                        trioMotionControl.DisconnectToController(IPAddress.Text);
                        trioMotionControl?.Dispose();
                        ConnectControllerButton.Content = "Connect";
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions gracefully
                    MessageBox.Show($"An error occurred: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("IP address is null or empty", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}

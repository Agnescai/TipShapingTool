using System;
using System.Collections.Generic;
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

        public TrioControllerConnection()
        {
            trioMotionControl = new TrioMotionControl();
            InitializeComponent();

        }


        private void ConnectControllerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(IPAddress.Text))
            {
                try
                {
                    // Attempt to open a connection
                    var connectionResult = trioMotionControl.IsControllerConnected();
                    if (connectionResult != null)
                    {
                        // Connect to the controller
                        trioMotionControl.ConnectToController(IPAddress.Text);
                        ConnectControllerButton.Content = "Disconnect";
                    }
                    else
                    {
                        // Disconnect from the controller
                        trioMotionControl.DisconnectToController(IPAddress.Text);
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

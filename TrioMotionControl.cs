using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;
using System.Windows;
using System.Windows.Threading;


      
namespace TipShaping
{
   

    public class TrioMotionControl : TrioPC
    {
        public delegate void PositionUpdateHandler(double x, double y, double z);
        public event PositionUpdateHandler TrioPositionUpdated;
        private DispatcherTimer positionUpdateTimer;
        public delegate void EnableButtonContentUpdatedHandler(double[] enableValue);
        public event EnableButtonContentUpdatedHandler TrioEnableButtonContentUpdated;

        int posUpdateInterval = 100;//ms

        public TrioMotionControl() 
        {
            // Initialize the timer
            positionUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(posUpdateInterval) // Update every 100 ms
            };
            positionUpdateTimer.Tick += PositionUpdateTimer_Tick;
        }

        private void PositionUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this.IsControllerConnected())
            {
                try
                {
                    double[] AxisPos = new double[3];
                    for (int i = 0; i < 3; i++)
                    {
                        this.GetAxisParameter(TrioMotion.TrioPC_NET.AxisParameter.MPOS, i, out AxisPos[i]);
                    }

                    // Raise the TrioPositionUpdated event on the main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TrioPositionUpdated?.Invoke(AxisPos[0], AxisPos[1], AxisPos[2]);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating positions: {0}", ex.Message);
                }
            }
        }


        internal void tripleVRs(int startingIndex, int number)
        {
            double temp;

            Console.WriteLine("Initialising VRs to tripple the original values ...");

            if (this.IsOpen(PortId.EthernetREMOTE))
            {
                for (int index = 0; index < number; index++)
                {
                    this.GetVr((index + startingIndex), out temp);

                    temp = 3 * temp;

                    this.SetVr((index + startingIndex), temp);
                }
            }
            else
            {
                Console.WriteLine("Not connected to controller");
            }
        }

        public void IndicateAxisEnable()
        {
            if (this.IsOpen(PortId.EthernetREMOTE))
            {
                double temp;
                double[] enableValue = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    this.GetAxisParameter(AxisParameter.AXIS_ENABLE, i, out temp);
                    enableValue[i] = temp;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TrioEnableButtonContentUpdated?.Invoke(enableValue);
                });

            }
            
        }


        public void ConnectToController(string ip_address)
        {
            this.SetHost(ip_address);

            if (this.Open(PortType.Ethernet, PortId.EthernetREMOTE))
            {
                Console.WriteLine("Connected to contoller on ip: {0}", this.HostAddress);
                
                // Start the position update timer
                if (!positionUpdateTimer.IsEnabled)
                {
                    positionUpdateTimer.Start();
                }
               

            }
            else
            {
                Console.WriteLine("Error in connecting to ip: {0}", this.HostAddress);
            }
        }

        public void DisconnectToController(string ipAddress)
        {
            try
            {
                if (this.IsOpen(PortId.EthernetREMOTE))
                {
                    // Stop the timer when disconnecting
                    if (positionUpdateTimer.IsEnabled)
                    {
                        positionUpdateTimer.Stop();
                    }

                    this.Close(PortId.EthernetREMOTE);
                    Console.WriteLine("Disconnected from controller at IP: {0}", ipAddress);
                }
                else
                {
                    Console.WriteLine("Controller is not connected. No action needed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while disconnecting: {0}", ex.Message);
            }
        }

        public void displayVRs(int startingIndex, int number)
        {
            double temp;

            Console.WriteLine("Reading VRs:");

            if (this.IsOpen(PortId.EthernetREMOTE))
            {
                for (int index = 0; index < number; index++)
                {
                    if (this.GetVr((index + startingIndex), out temp))
                    {
                        Console.WriteLine("VR({0}) = {1}", (index + startingIndex), temp);
                    }
                }
            }
            else
            {
                Console.WriteLine("Not connected to controller");
            }
        }

        public bool IsControllerConnected()
        {
            return this.IsOpen(PortId.EthernetREMOTE);
        }
    }

       


    }

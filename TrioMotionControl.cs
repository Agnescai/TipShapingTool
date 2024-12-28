using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;


namespace TipShaping
{
   

    public class TrioMotionControl : TrioPC
    {
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

        public void ConnectToController(string ip_address)
        {
            this.SetHost(ip_address);

            if (this.Open(PortType.Ethernet, PortId.EthernetREMOTE))
            {
                Console.WriteLine("Connected to contoller on ip: {0}", this.HostAddress);
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

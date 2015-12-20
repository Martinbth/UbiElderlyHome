using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets;
using Phidgets.Events;


namespace ConsoleApplication3
{
    public class Pillbox
    {
        InterfaceKit ifKit;
        bool[] pillDay = {false, false, false, false, false, false, false};
        bool takenPills = false;

        public Pillbox()
        {
            try
            {
                //Initialize the InterfaceKit object
                ifKit = new InterfaceKit();

                //Hook the basica event handlers
                ifKit.Attach += new AttachEventHandler(ifKit_Attach);
                ifKit.Detach += new DetachEventHandler(ifKit_Detach);
                ifKit.Error += new ErrorEventHandler(ifKit_Error);

                //Hook the phidget spcific event handlers
                ifKit.InputChange += new InputChangeEventHandler(ifKit_InputChange);
                ifKit.OutputChange += new OutputChangeEventHandler(ifKit_OutputChange);
                ifKit.SensorChange += new SensorChangeEventHandler(ifKit_SensorChange);

                //Open the object for device connections
                ifKit.open();

                //Wait for an InterfaceKit phidget to be attached
                Console.WriteLine("Waiting for InterfaceKit to be attached...");
                ifKit.waitForAttachment();

                //string input = "";

                //Reset all LEDS connected to the pillbox
                for (int i = 0; i < 8; i++)
                {
                    ifKit.outputs[i] = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /*
         * time2TakePills
         * args: int day is the day from 0 (monday) to 6 (sunday) on which to take the pill
         * 
         * */
        public void time2TakePills(int day)
        {
            Console.WriteLine("Time to take medicine from box: " + day);
            ifKit.outputs[day] = true;
            pillDay[day] = true;
        }

        /*
         * pillTaken()
         * args: int day is the day from 0 (monday) to 6 (sunday) on which the pill was taken
         * 
         * */
        public void pillTaken(int day)
        {
            ifKit.outputs[day] = false;
            pillDay[day] = false;
        }

        /**
         *  close()
         *  closes the connection between the computer and the interfacekit (pillbox)
         * */
        public void close()
        {
            //Reset all LEDS connected to the pillbox
            for (int i = 0; i < 8; i++)
            {
                ifKit.outputs[i] = false;
            }
            //Close the connection to the interfacekit
            ifKit.close();
            ifKit = null;
        }
        
        //Attach event handler...Display the serial number of the attached InterfaceKit 
        //to the console
        void ifKit_Attach(object sender, AttachEventArgs e)
        {
            Console.WriteLine("InterfaceKit {0} attached!",
                                e.Device.SerialNumber.ToString());
        }

        //Detach event handler...Display the serial number of the detached InterfaceKit 
        //to the console
        void ifKit_Detach(object sender, DetachEventArgs e)
        {
            Console.WriteLine("InterfaceKit {0} detached!",
                                e.Device.SerialNumber.ToString());
        }

        //Error event handler...Display the error description to the console
        void ifKit_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Description);
        }

        //Input Change event handler...Display the input index and the new value to the 
        //console
        void ifKit_InputChange(object sender, InputChangeEventArgs e)
        {
            Console.WriteLine("Input index {0} value (1)", e.Index, e.Value.ToString());
        }

        //Output change event handler...Display the output index and the new valu to 
        //the console
        void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
            Console.WriteLine("Output index {0} value {0}", e.Index, e.Value.ToString());
        }

        //Sensor Change event handler...Display the sensor index and it's new value to 
        //the console
        void ifKit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            //Console.WriteLine("Sensor index {0} value {1}", e.Index, e.Value);
            if(e.Value < 999 && e.Value > 10){
                Console.WriteLine("Pressed a analog button");
                if (pillDay[e.Index])
                {
                    takenPills = true;
                    Console.WriteLine("Corresponding button -> day pressed");
                    pillTaken(e.Index);
                }
                else
                {
                    Console.WriteLine("Wrong button pressed: ..." + e.Index);
                }
            }
        }

        //returns status of if taken pills or not
        public bool getTakenPills()
        {
            return takenPills;
        }
    }
}

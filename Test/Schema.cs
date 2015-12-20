using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;

namespace schema { 
    public class Person {
        
        //Schedual checklist and the name of the person
        public TimeSpan startWakeUp = new TimeSpan(07, 0, 0);
        public TimeSpan stopWakeUp = new TimeSpan(08, 0, 0);

        public TimeSpan startBrush = new TimeSpan(07, 30, 0);
        public TimeSpan stopBrush = new TimeSpan(08, 0, 0);

        public TimeSpan startShower = new TimeSpan(08, 0, 0);
        public TimeSpan stopShower = new TimeSpan(09, 0, 0);

        public TimeSpan startTakePill = new TimeSpan(17, 0, 0);
        public TimeSpan stopTakePill = new TimeSpan(21, 0, 0);
      
        public Boolean brushedTeeth= false;
        public Boolean takenAShower = false;
        public Boolean wokenUp = false;
        public Boolean takenPills = false;
      
        public bool [] actions;
        
        public Person(){
            actions = new bool[4];
            }
        /*
         *  see whats next actions should be by using the function "mname.Status(input)"
         *      Input: 1 dimensional bool array that describes if a sensor has been activated or not with true/false
         * inputarray[0] : Wake up Sensor : false defult. true om RFID har blivit aktiverad av dörren
         * inputarray[1] : brush teeth Sensor : false defult. true = RFID has been activated from toothbrush
         * inputarray[2] : showerSensor : false defult. true = RFID has been activated by showercrane
         * inputarray[3] : Take pill sensor : false defult. true = force sensor has been activated
         *      
         *
         * Output: 1 dimensional bool array that has the info of what actions to make by using true/false statments
         * outputarray[0] : wake up  : false as defult = off. true = activate the sensor (display)
         * outputarray[1] : brush teeth  : false defult. true = activate the sensors (display)
         * outputarray[2] : shower: false defult. true = activate the sensors (display)
         * outputarray[3] : Take pill  : false defult. true = activate the sensors (display, LEDS on box)
         */
        public  bool[] Status(bool [] sensorContact){
            //Checking if there is a new day
            CheckIfNewDay();
            
            //Turn on text to wake up if the time is right and person has not gone out from room
            if (!wokenUp){
                bool wakeUpAlarm = TimeBetween(DateTime.Now, startWakeUp, stopWakeUp);
                if(wakeUpAlarm){
                    actions[0] = true;
                    if (sensorContact[0]){
                        // Action: turn of wake up text on display for waking up
                        wokenUp = true;
                        actions[0] = false;
                    }
                }
            }
            else if (!brushedTeeth){
                 bool brushTeethAlarm = TimeBetween(DateTime.Now, startBrush, stopBrush);
                 if(brushTeethAlarm){
                     actions[1] = true;
                     if (sensorContact[1]){
                         // Action : turn of text on display for brushing teeth
                         brushedTeeth = true;
                         actions[1] = false;
                     }
                 }
            }
            else if (!takenAShower){
                bool showerAlarm = TimeBetween(DateTime.Now, startShower, stopShower);
                if (showerAlarm) { 
                    actions[2] = true;
                    if (sensorContact[2]){
                        // Action : turn of text on display for taking a shower
                        takenAShower = true;
                        actions[2] = false;
                    }
                }
            }
            else if (!takenPills){
                bool takePillAlarm = TimeBetween(DateTime.Now, startTakePill, stopTakePill);
                if (takePillAlarm) { 
                    actions[3] = true;
                    if (sensorContact[3]){
                        // Action : turn of text on display for taking the pills
                        takenPills = true;
                        actions[3] = false;
                    } 
                }
            }
            return actions;
        }
        //checks if there is a new day and set the schores status to false
        public void CheckIfNewDay(){
            if(brushedTeeth && wokenUp && takenAShower && takenPills){
                brushedTeeth= false;
                takenAShower = false;
                wokenUp = false;
                takenPills = false;
                Console.WriteLine("NEW DAY");
            }
        }
        //Checks if the current time and a event is in the same timespan
        static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            //Console.WriteLine(now);
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
       
    }

}





using System;
using System.Text;
using System.Configuration;
using schema;
using ConsoleApplication3;
using ConsoleApplication1;

public class Program
{
    //Puts the sensors status true if the RFID sensor has detected any of the tags
    public static bool[] sensorHandler(bool[] detected, bool[] sensor)
    {
        if (detected[0])
        {
            sensor[0] = true;
        }
        if (detected[1])
        {
            sensor[1] = true;
        }
        if (detected[2])
        {
            sensor[2] = true;
        }
        return sensor;
    }
    //handles the activation/deactivation for the LEDS depending on what day it is.
    public static void pillboxHandler(Pillbox pb,bool[] actions,bool[] sensors)
    {
        DayOfWeek day = DateTime.Now.DayOfWeek;
          if (pb.getTakenPills())
            {
                actions[3] = false;
            }

        //Activate the the LED that represent the current day
        if (actions[3]){
                switch (day){
                case DayOfWeek.Monday:
                    Console.WriteLine("Case Monday");
                    pb.time2TakePills(0);
                    break;
                case DayOfWeek.Tuesday:
                    Console.WriteLine("Case Tuesday");
                    pb.time2TakePills(1);
                    break;
                case DayOfWeek.Wednesday:
                    Console.WriteLine("Case Wednesday");
                    pb.time2TakePills(2);
                    break;
                case DayOfWeek.Thursday:
                    Console.WriteLine("Case Thursday");
                    pb.time2TakePills(3);
                    break;
                case DayOfWeek.Friday:
                    Console.WriteLine("Case Friday");
                    pb.time2TakePills(4);
                    break;
                case DayOfWeek.Saturday:
                    Console.WriteLine("Case Saturday");
                    pb.time2TakePills(5);
                    break;
                case DayOfWeek.Sunday:
                    Console.WriteLine("Case Sunday");
                    pb.time2TakePills(6);
                    break;
            }
        }
    }

    public static void Main()
    {
        bool[] detected;
        //Constructs a person with a already known schema
        Person martin = new Person();

        //Creates pillbox sensor handler
        Pillbox pb = new Pillbox();

        //Creates lcd/RFID sensor handler
        LCDandRFID lcdRFID = new LCDandRFID();
        lcdRFID.StartLCDRFID();
        
        //Array of active/inactive senors
        bool[] sensors = { false, false, false, false};
        
        //Array of what sensors that should activate/be disabled
        bool[] actions = martin.Status(sensors);

        //checking status of the schedual every 5 min and act accordingly
        var timer = new System.Threading.Timer((e) =>
        {
            //check what tags that has been detected by the RFID sensor
            detected=lcdRFID.getDetectedTags();
            //Checks for what should be set to true.
            sensors=sensorHandler(detected, sensors);
            //get instruction on what actions to take
            actions = martin.Status(sensors);
            //activates pillbox LEDS if its time for that the persons schedual
            pillboxHandler(pb, actions,sensors);
            //sends instruction for the LCD for what to do next
            lcdRFID.setOnOffLcdTasks(actions);
            //Console.WriteLine("element {0} have the value {1}", 0, actions[0]);
            //Console.WriteLine("element {0} have the value {1}", 1, actions[1]);
            //Console.WriteLine("element {0} have the value {1}", 2, actions[2]);
            //Console.WriteLine("element {0} have the value {1}", 3, actions[3]);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        // Keep the console window open in debug mode.
        Console.ReadKey();
 
    }
}


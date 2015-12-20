using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets;
using Phidgets.Events;
using System.Collections;

namespace ConsoleApplication1
{
    public class LCDandRFID
    {
        // Skapa en hashtabell för att hantera olika objekt som innehar rfid
        static Hashtable rfidItems = new Hashtable();
        static string dbLocation = "../../tagDb.tags";
        //Skapa en array med karaktärer som inte är tillåtna i ett rfidnamn 
        //(dubbla \ så att den inte tror att nåt händer)
        static char[] illegalChars = { ' ', '/', '\\' };
        // Phidgets
        static RFID rfid = new RFID();
        //static InterfaceKit ifKit = new InterfaceKit();
        static TextLCD tLCD;
        //[0]=Door, [1]=Toothbrush, [2]=shower
        static bool[] detectedTags = { false, false, false };


        public void StartLCDRFID()
        {
            try
            {
                // Används för att känna av när programmet stängs ned
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit); 
                // Läser in "databasen" med taggar
                readTagFile(dbLocation);
                // Initierar phidgets ifkit = leds och digitala outputs
                rfid_init();
                //ifKit_init();
                rfid.open();
                //ifKit.open();
                tLCD_init();
                tLCD.open();

                // Vänta på att rfidn ska bli ansluten
                Console.WriteLine("Waiting for RFID-reader...");
                rfid.waitForAttachment();
                //Wait for an InterfaceKit phidget to be attached
                Console.WriteLine("Waiting for InterfaceKit to be attached...");
                //vänta på lcd
                if (!tLCD.Attached)
                {
                    Console.WriteLine("Waiting for TextLCD to be attached....");
                    tLCD.waitForAttachment();
                }

                try
                {
                    tLCD.Screen = 0;
                    tLCD.ScreenSize = TextLCD.ScreenSizes._2x20;
                }
                catch (Exception e)
                {
                    printText(e.Message);
                }

                // När phidgets är korrekta slås på
                rfid.Antenna = true;
                rfid.LED = true;

                //Console.WriteLine("Press any key to end..:");
                //string input = "nug";
            }
            catch (PhidgetException ex)
            {
                Console.WriteLine(ex.Description);
            }
        }

        /*
         * RFID funktioner följer
         * 
         * */
        static void rfid_init()
        {
            // Initialisera händelsehanterarna för phidgets
            rfid.Attach += new AttachEventHandler(rfid_Attach);
            rfid.Detach += new DetachEventHandler(rfid_Detach);
            rfid.Error += new ErrorEventHandler(rfid_Error);
            rfid.Tag += new TagEventHandler(rfid_Tag);
            rfid.TagLost += new TagEventHandler(rfid_TagLost);
        }

        // om en rfidläsare ansluts
        static void rfid_Attach(object sender, AttachEventArgs e)
        {
            printText("RFID reader " + e.Device.SerialNumber.ToString() + " attached.");
        }

        // om rfidläsaren dras ut
        static void rfid_Detach(object sender, DetachEventArgs e)
        {
            printText("RFID reader " + e.Device.SerialNumber.ToString() + " detached.");
        }

        // om det blir ett error från läsaren
        static void rfid_Error(object sender, ErrorEventArgs e)
        {
            printText(e.Description);
        }

        // om en rfidtag plötsligt hittas
        static void rfid_Tag(object sender, TagEventArgs e)
        {
            printText("Tag " + e.Tag + " scanned.");
            checkRFID(e.Tag);
            registerDetectedRFID(e.Tag);
        }

        // Om taggen inte hittas längre av rfid-läsaren
        static void rfid_TagLost(object sedner, TagEventArgs e)
        {
            printText("Tag " + e.Tag + " lost.");
        }

        /*
         *  funktioner för lcd följer
         * 
         * */
        static void tLCD_init()
        {
            tLCD = new TextLCD();
            tLCD.Attach += new AttachEventHandler(tLCD_Attach);
            tLCD.Detach += new DetachEventHandler(tLCD_Detach);
            tLCD.Error += new ErrorEventHandler(tLCD_Error);
        }

        //attach event handler, we'll output the name and serial number of the TextLCD
        //that was attached
        static void tLCD_Attach(object sender, AttachEventArgs e)
        {
            TextLCD attached = (TextLCD)sender;
            string name = attached.Name;
            string serialNo = attached.SerialNumber.ToString();

            Console.WriteLine("TextLCD name:{0} serial No.: {1} Attached!", name,
                                    serialNo);
        }

        //Detach event handler, we'll output the name and serial of the phidget that is
        //detached
        static void tLCD_Detach(object sender, DetachEventArgs e)
        {
            TextLCD detached = (TextLCD)sender;
            string name = detached.Name;
            string serialNo = detached.SerialNumber.ToString();

            Console.WriteLine("TextLCD name:{0} serial No.: {1} Detached!", name,
                                    serialNo);
        }

        //TextLCD error event handler, we'll just output any error data to the console
        static void tLCD_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("LCD Error: e.Description");
        }

        // Funktion för att skriva ut text på lcd
        static void tLCD_print(string text1, string text2)
        {
            if (tLCD.Attached)
            {
                //row0.Clear();
                try
                {
                    tLCD.screens[0].Backlight = true;
                    tLCD.screens[0].rows[0].DisplayString = text1;
                    tLCD.screens[0].rows[1].DisplayString = text2;
                }catch(Exception e){
                    printText("Exception in printing: " + e.Message);
                }
                System.Threading.Thread.Sleep(4990);
                tLCD.screens[0].Backlight = false;
                tLCD_clear();
            }
        }

        // Annan konstruktor för att skriva ut på lcdn med bara en rad, första raden blir något hårdkodat
        static void tLCD_print(string text)
        {
            tLCD_print("tag recognized:", text);
        }

        // Tar bort all text från skärmen
        static void tLCD_clear()
        {
            tLCD.screens[0].rows[0].DisplayString = "";
            tLCD.screens[0].rows[1].DisplayString = "";
        }
        
        //Attach event handler...Display the serial number of the attached InterfaceKit 
        //to the console
        static void ifKit_Attach(object sender, AttachEventArgs e)
        {
            Console.WriteLine("InterfaceKit {0} attached!",
                                e.Device.SerialNumber.ToString());
        }

        //Detach event handler...Display the serial number of the detached InterfaceKit 
        //to the console
        static void ifKit_Detach(object sender, DetachEventArgs e)
        {
            Console.WriteLine("InterfaceKit {0} detached!",
                                e.Device.SerialNumber.ToString());
        }

        //Error event handler...Display the error description to the console
        static void ifKit_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Description);
        }

        //Input Change event handler...Display the input index and the new value to the 
        //console
        static void ifKit_InputChange(object sender, InputChangeEventArgs e)
        {
            Console.WriteLine("Input index {0} value (1)", e.Index, e.Value.ToString());
        }

        //Output change event handler...Display the output index and the new valu to 
        //the console
        static void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
            //Console.WriteLine("Output index {0} value {0}", e.Index, e.Value.ToString());
        }

        //Sensor Change event handler...Display the sensor index and it's new value to 
        //the console
        static void ifKit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            Console.WriteLine("Sensor index {0} value {1}", e.Index, e.Value);
        }

        /**
         *  Slut på interfacekit
         * */
        static void printText(string text)
        {
            Console.WriteLine("debug:" + text);
        }

        // Kollar om rfid-taggen som lästs in finns i en hashtabell och skriver isåfall ut det
        static void checkRFID(string tag)
        {
            string currentTime = DateTime.Now.ToString("h:mm:ss tt");
            try
            {
                // Om den hittar taggen i databasen så skriv ut det på skärmen
                if (rfidItems.Contains(tag))
                {
                    printText("found a " + rfidItems[tag] + ". stamp: " + currentTime);
                    tLCD_print((string)rfidItems[tag]);
                }
                // Hittar den inte taggen i databasen så fråga anändaren om den vill läggas till, och lägg isåfall till den
                else
                {
                    printText("unrecognized rfid, do you want to add it to the database nigga? y/n :");
                    string ans = Console.ReadLine();
                    if (ans.Equals("y"))
                    {
                        bool noIllegalBusiness = false;
                        while (!noIllegalBusiness) {
                            noIllegalBusiness = true;
                            printText("enter name for RFID:");
                            ans = Console.ReadLine();
                            foreach (char ilCh in illegalChars)
                            {
                                if(ans.Contains(ilCh))
                                {
                                    printText("illegal character");
                                    noIllegalBusiness = false;
                                }
                            }
                        }
                        rfidItems.Add(tag, ans);
                        addTagFile(dbLocation, tag, ans);
                        printText("tag added!");
                    }
                }
            }catch(ArgumentNullException ex){
                printText(ex.Message);
            }
        }

        /*
        *  Läser av alla taggar i taggdatabasen och sätter in dem i en hashtabell
        * */
        static void readTagFile(string location)
        {
            string[] db = System.IO.File.ReadAllLines(location);
            foreach(string tag in db)
            {
                string[] tagDb = tag.Split(' ');
                if (tagDb.Length == 2)
                {
                    rfidItems.Add(tagDb[0], tagDb[1]);
                }
                else
                {
                    printText("error in the database file..");
                }
            }
        }

        /*
         *  Lägger till en rfid-tagg i textdatabasen
         * */
        static void addTagFile(string location, string key, string value)
        {
            System.IO.File.AppendAllText(location, Environment.NewLine + key + ' ' + value);
        }

        /**
         * kod som körs när programmet stängs ned.
         * 
         * */
        static void OnProcessExit(object sender, EventArgs e)
        {
            //rfid.LED = false;

            rfid.close();
            tLCD.close();
           // ifKit.close();
            rfid = null;
            Console.WriteLine("Goodbye");
        }

        static void registerDetectedRFID(string e) {
            if (e==("01022c6430")) {
                detectedTags[0] = true;
            }
            else if (e==("0f000a1087"))
            {
                detectedTags[1] = true; 
            }
            else if (e==("0f000adcad"))
            {
                detectedTags[2] = true;
            }

        }
        public bool[] getDetectedTags() {
            return detectedTags;
        }

        public void setOnOffLcdTasks(bool[] actions)
        {
            if (actions[0])
            {
                string text1 = "WAKE UP TIME";
                string text2 = "WE DA BEST MUSIC";
                tLCD_print(text1, text2);
            }
            else if (actions[1])
            {
                string text1 = "BRUSH TEETH";
                string text2 = "U SMART";
                tLCD_print(text1, text2);
            }
            else if (actions[2])
            {
                string text1 = "TIME FOR SHOWER";
                string text2 = "NICE";
                tLCD_print(text1, text2);
            }
            else if (actions[3])
            {
                
                string text1 = "DRUG TIME";
                string text2 = "420";
                tLCD_print(text1, text2);
            }
        }
    }   
}

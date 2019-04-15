/* Author: Benjamin Sanders
 *         Co-Op | First Term | IT/OPT | Innovations
 *         bosanders@crimson.ua.edu
 *         
 * Purpose: Serial Comminication Library
 *          This library was created to read, manipulate, and usitlize data sent over a UART connection on a serial port. A
 *          basic understanding of serial comminication is recommended when reading through or using this library. While this
 *          library was created specifically for use with the ZX Gesture Sense, it can be used for any serial communication with slight modification.
 *          
 *          ZX Gesture Sense Datasheet: https://cdn.sparkfun.com/assets/learn_tutorials/3/4/5/XYZ_Interactive_Technologies_-_ZX_SparkFun_Sensor_Datasheet.pdf 
 * */


using System;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace GestureSenseLibrary
{
    ///<summary>
    /// This class contains functions that deal with opening and reading from Serial Ports.
    ///</summary>
    public class PortReading
    {
        /// <summary>
        /// This function creates a connection to a port to be read from. Note, you must open the port (yourPort.Open()) after calling
        /// this function in your program.
        /// </summary>
        /// <param name="portName">Port name and number. Ex: COM4</param>
        /// <param name="baud">Speed at which the port is read from. Default is 115200</param>
        /// <returns>The newly created port</returns>
        public static SerialPort CreatePort(string portName, int baud = 115200)
        {
            // Default SerialPort settings.
            var port = new SerialPort(portName);
            port.DataBits = 8;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.BaudRate = baud;

            return port;
        }

        /// <summary>
        /// This function reads from an open COM port and puts the data read into a queue. This function works
        /// for one section of data at a time and should not be used for applications that need data read in
        /// constantly.
        /// </summary>
        /// <param name="port">Serial port to read from</param>
        /// <returns>A queue populated with data from the port</returns>
        public static Queue ReadSerialPort(SerialPort port)
        {
            var myData = new Queue();
            var val = 0;

            // If port is not open.
            if (!port.IsOpen)
            {
                Console.Out.WriteLine("port is not open!");
                return null;
            }

            // Reads initial data.
            val = port.ReadByte();

            Console.WriteLine("Reading from port...");

            while (val != 255)
            {
                // Uncomment below to see all data read from port.
                // Console.WriteLine(val);

                // If value read from port is 255, or the value is between 0 and 252 (end of read).
                if ((val == 255) || ((val < 252) && (val > 0)))
                {
                    // Uncomment below to see all data read from port.
                    // Console.WriteLine(b);
                    myData.Enqueue(val);
                }

                // Reads next byte.
                val = port.ReadByte();

                // Clears the queue if it grows too large due to an extended read.
                if ((myData.Count > 300) && (val <= 240))
                {
                    Console.WriteLine("Clearning queue for overflow protection. Please remove any obstructions from sensor's FOV.");
                    myData.Clear();
                }
            }
            return myData;
        }

        /// <summary>
        /// This function reads in data from a port into a queue. This function is meant
        /// to be used to constantly read in data from a port.
        /// </summary>
        /// <param name="myData">Queue, passed by reference, to be populated by the function</param>
        /// <param name="port">Port to be read from at 115200 baud</param>
        public static void ReadPortToQueue(ref Queue myData, SerialPort port)
        {
            var val = 0;

            // If port is not open.
            if(!port.IsOpen)
            {
                Console.Out.WriteLine("port is not open!");
                return;
            }

            // Reads initial data.
            val = port.ReadByte();

            Console.WriteLine("Reading from port...");

            // While value is not 0, to keep loop running constnatly.
            while (val != 0)
            {
                // While value is not 255 which is the End Of Transmissoin byte.
                while (val != 255)
                {
                    // Uncomment below to see all data read from port.
                    // Console.WriteLine(val);

                    // If value is between 0 and 252.
                    if ((val <= 252) && (val > 0))
                    {
                        // Uncomment below to see all data read from port.
                        // Console.WriteLine(val);
                        myData.Enqueue(val);
                    }
                    // Reads next byte.
                    val = port.ReadByte();
                }
                // Waits for incoming data and reads it.
                val = port.ReadByte();
            }
            return;
        }

        /// <summary>
        /// This function reads data from a port and ignores all data until a gesture code is sent
        /// </summary>
        /// <param name="port">Open serial port to read from</param>
        /// <returns>Int value related to a gesture</returns>
        public static int ReadGestureCode(SerialPort port)
        {
            var val = 0;

            // If port is not open.
            if (!port.IsOpen)
            {
                Console.Out.WriteLine("port is not open!");
                return -1;
            }

            Console.WriteLine("Reading from port...");

            // Reads initial data.
            val = port.ReadByte();

            // While value is not 255 which is the End Of Transmissoin byte.
            while (val != 255)
            {
                // Uncomment below to see all data read from port.
                // Console.WriteLine(val);

                // If value read from port is 252, next value is a gesture.
                if (val == 252)
                {
                    // Uncomment below to see all data read from port.
                    // Console.WriteLine(val);
                    return port.ReadByte();
                }
                // Reads next byte.
                val = port.ReadByte();
            }
            return -1;
        }

        /// <summary>
        /// This function reads gesture codes and the following parameter from a serial port into a queue
        /// </summary>
        /// <param name="port">Open serial port to read from</param>
        /// <returns>Queue holding a gesture code and parameter</returns>
        public static Queue ReadGestureCodeAndParam(SerialPort port)
        {
            var myQueue = new Queue();
            var val = 0;

            // If port is not open.
            if (!port.IsOpen)
            {
                Console.Out.WriteLine("port is not open!");
                return null;
            }

            Console.WriteLine("Reading from port...");

            // Reads initial data.
            val = port.ReadByte();

            // While value is not 255 which is the End Of Transmissoin byte.
            while (val != 255)
            {
                // Uncomment to see all data read from port.
                // Console.WriteLine(val);

                // If value read from port is 252, next value is a gesture.
                if (val == 252)
                {
                    // Reads next byte.
                    val = port.ReadByte();

                    // While value is between -1 and 241.
                    while ((val >= 0) && (val <= 240))
                    {
                        myQueue.Enqueue(val);
                        // Uncomment to see values read from port.
                        Console.WriteLine(val);
                        val = port.ReadByte();
                    }
                    return myQueue;
                }
                // Waits for incoming data and reads it.
                val = port.ReadByte();
            }
            return myQueue;
        }
    }

    ///<summary>
    /// This class contains functions that deal with processing and using data stored in queues.
    ///</summary>
    public class DataProcessing
    {
        /// <summary>
        /// This function takes in a queue of raw port data and splits it into x and z values which are stored in an x and z queue. This funciton is meant to
        ///     be run each time it is needed.
        /// </summary>
        /// <param name="myData">Queue of raw data</param>
        /// <param name="xQueue">Queue to store x values</param>
        /// <param name="zQueue">Queue to store z values</param>
        public static void SortDataXAndZ(Queue myData, ref Queue xQueue, ref Queue zQueue)
        {
            // Checks for empty queue.
            if (myData.Count == 0)
            {
                return;
            }

            // While queue is not empty.
            while (myData.Count > 0)
            {
                // Collect data group.
                while (myData.Count > 0 && (int)myData.Peek() != 255)
                {
                    // If value in queue is 250, read x.
                    if ((int)myData.Peek() == 250)
                    {
                        // Dequeues message byte.
                        myData.Dequeue();
                        // Moves x value into x queue.
                        xQueue.Enqueue(myData.Dequeue());
                    }
                    // If value in queue is 251, read z.
                    else if ((int)myData.Peek() == 251)
                    {
                        // Dequeue message byte.
                        myData.Dequeue();
                        // Moves z value into z queue.
                        zQueue.Enqueue(myData.Dequeue());
                    }
                    else
                    {
                        // Dequeue message byte.
                        myData.Dequeue();
                    }
                }
            }
            return;
        }

        /// <summary>
        /// This function takes in data from a queue and cleans it to be used by a scrolling function for scrolling applications.
        /// </summary>
        /// <param name="myQueue">Queue of raw data</param>
        /// <param name="newQueue">Queue of clean data</param>
        public static void SortDataForScroll(ref Queue myQueue, ref Queue newQueue)
        {
            // Waits for queue to be filled.
            while (myQueue.Count == 0)
            {
                Thread.Sleep(25);
            }

            var readVal = 0;

            // While the queue is not empty.
            while (myQueue.Count > 0)
            {
                readVal = (int)myQueue.Dequeue();

                // Next value is x value.
                if (readVal == 250)
                {
                    // If myQueue isn't empty and the next value is less than 240.
                    if (myQueue.Count > 0 && (int)myQueue.Peek() < 240)
                        // Move the value into the temp variable.
                        readVal = (int)myQueue.Dequeue();

                    // Moves value into the queue.
                    newQueue.Enqueue(readVal);
                }
                // Next value is z value.
                else if (readVal == 251)
                {
                    // If myQueue isn't empty and the next value is less than 240.
                    if (myQueue.Count > 0 && (int)myQueue.Peek() < 240)
                        // Moves the value into the temp variable.
                        readVal = (int)myQueue.Dequeue();

                    // Resets value to be positive.
                    readVal *= -1;

                    // Moves value into queue.
                    newQueue.Enqueue(readVal);
                }

                else if (myQueue.Count != 0)
                {
                    myQueue.Dequeue();
                }

                while (myQueue.Count == 0)
                {
                    Thread.Sleep(25);
                }
            }
        }

        /// <summary>
        /// Processes data from queue into a gesture
        /// -1: no gesture
        /// 1: right swipe (value increased)
        /// 2: left swipe (value decreased
        /// 3: left bump (value increased then decreased)
        /// 4: right bump (value decreased then increased)
        /// </summary>
        /// <param name="xQueue">Queue of cleaned sensor data</param>
        /// <returns>Int related to a gesture</returns>
        public static int ProcessGesture(Queue xQueue)
        {
            // Checks if queue is empty.
            if (xQueue.Count == 0)
            {
                return -1;
            }
            var sum = 0;
            var traverse = 0;
            int xMax = (int)xQueue.Peek();
            int xMin = (int)xQueue.Peek();
            traverse = xMax;
            int previous = traverse;
            int firstVal = traverse;
            int secondVal = traverse;
            var lastVal = 0;
            var counter = 0;
            int numVals = xQueue.Count;

            // While the queue is not empty.
            while (xQueue.Count > 0)
            {
                // Removes first byte to decrease reading inacuracy.
                if (counter == 0)
                {
                    // Dequeue first byte.
                    xQueue.Dequeue();

                    // If the queue is holding more than 3 values.
                    if (xQueue.Count > 3)
                    {
                        // Resetting second byte to be first byte.
                        traverse = (int)xQueue.Peek();
                        firstVal = traverse;
                        xMax = traverse;
                        xMin = traverse;
                    }
                    else
                    {
                        return -1;
                    }
                }

                // Find max and min.
                if (traverse > xMax)
                {
                    xMax = traverse;
                }
                else if (traverse < xMin)
                {
                    xMin = traverse;
                }

                // Add if the value is increasing.
                if (traverse >= previous)
                {
                    sum += traverse;
                }
                // Subtract if the value is decreasing.
                else
                {
                    sum -= traverse;
                }

                // Stores the previous value.
                previous = traverse;

                // Removes current values from queue.
                xQueue.Dequeue();

                // If queue isn't empty.
                if (xQueue.Count != 0)
                    // Updates current value.
                    traverse = (int)xQueue.Peek();

                counter++;

                // If current value is second value in queue.
                if (counter == 1)
                    secondVal = traverse;
            }
            lastVal = previous;

            // If the first value (and second value for redundency) is less than
            // the max, then the reflector entered and exited from left.
            if ((firstVal < xMax) && (secondVal < xMax) && (lastVal < xMax))
            {
                Console.WriteLine("Gesture: BUMP_LEFT");
                Console.WriteLine("*******************");
                return 3;
            }
            // Values increased.
            else if (sum > xMax)
            {
                Console.WriteLine("Gesture: RIGHT_SWIPE");
                Console.WriteLine("*******************");
                return 1;
            }
            // If the first value (and second value for redundency) is greater than the
            // min, then the reflector entered and exited from right.
            else if ((secondVal > xMin) && (lastVal > xMin))
            {
                Console.WriteLine("Gesture: BUMP_RIGHT");
                Console.WriteLine("*******************");
                return 4;
            }
            // Values decreased.
            else if (sum < xMin)
            {
                Console.WriteLine("Gesture: LEFT_SWIPE");
                Console.WriteLine("*******************");
                return 2;
            }
            return -1;
        }

        /// <summary>
        /// Sends keystoke based off gesture code received.
        /// </summary>
        /// <param name="gestureCode">Int relating to a gesture</param>
        public static void SendKeystroke(int gestureCode)
        {
            switch (gestureCode)
            {
                // Right swipe.
                case 1:
                    // Click right once.
                    System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
                    break;

                // Left swipe.
                case 2:
                    // Click left once.
                    System.Windows.Forms.SendKeys.SendWait("{LEFT}");
                    break;

                // Right bump.
                case 3:
                    // Click left twice.
                    System.Windows.Forms.SendKeys.SendWait("{LEFT 2}");
                    break;

                // Left bump.
                case 4:
                    // Click right twice.
                    System.Windows.Forms.SendKeys.SendWait("{RIGHT 2}");
                    break;

                // Down swipe.
                case 10:
                    // Add keystoke to send here
                    System.Windows.Forms.SendKeys.SendWait("");
                    break;

                // Add more cases here for more functionality.

                default:
                    Console.WriteLine("Error: No Gesture");
                    break;
            }
        }

        /// <summary>
        /// Takes in cleaned data from sensor and identifies the direction to scroll
        /// </summary>
        /// <param name="newQueue">queue of cleaned senor data</param>
        public static void ProcessScroll(ref Queue newQueue)
        {
            var xVal = 0;
            var zVal = 0;
            var count = 0;
            var readVal = 0;
            var returnVal = 0;
            var flag = 0;

            // Waits for queue to be filled.
            while (newQueue.Count == 0)
            {
                Thread.Sleep(25);
            }

            // While the queue is not empty.
            while (newQueue.Count > 0)
            {
                // Throws away 11 values before reading two to slow down scrolling.
                while (count < 11)
                {
                    if (newQueue.Count != 0)
                    {
                        // Protection against NULL Ref Exception.
                        try
                        {
                            readVal = (int)newQueue.Dequeue();
                        } catch (NullReferenceException)
                        {
                            Console.WriteLine("Error: Null Reference Exception");
                        }

                        // Uncomment to see data being processed.
                        // Console.WriteLine(readVal);

                        // If negative, then it is a z value. Else, it is positive and an x value.
                        if (readVal < 0)
                        {
                            // Resets value to positive.
                            readVal *= -1;
                            zVal = readVal;
                        }
                        else if (readVal > 0)
                        {
                            xVal = readVal;

                            // Set flag.
                            flag++;
                        }
                    }
                    count++;
                }

                count = 0;

                // Default 40 90 150 40/50

                // Adjust vales to change the size of the scrolling hitboxes. A graph showing the hitboxes
                // is available in the info documentation.
                // Scroll down.
                if ((zVal <= 30) && (xVal > 110) && (xVal < 140))
                {
                    returnVal = SendKeystrokeToScroll(1);
                }
                // Scroll up.
                else if ((zVal >= 70) && (xVal > 110) && (xVal < 140))
                {
                    returnVal = SendKeystrokeToScroll(2);
                }
                // Scroll left.
                else if ((flag != 3) && (xVal <= 110) && (zVal > 30) && (zVal < 70))
                {
                    returnVal = SendKeystrokeToScroll(3);
                }
                // Scroll right.
                else if ((flag != 3) && (xVal >= 140) && (zVal > 30) && (zVal < 70))
                {
                    returnVal = SendKeystrokeToScroll(4);
                }

                // Resets every third byte read.
                if(flag >= 3)
                {
                    flag = 0;
                    //Console.Clear();
                }
                
                if((returnVal == -1) || (newQueue.Count > 300))
                {
                    newQueue.Clear();
                }
                else
                {
                    returnVal = 0;
                }

                // Waits for queue to be filled.
                while (newQueue.Count == 0)
                {
                    Thread.Sleep(25);
                }
                //Console.Clear();
            }
            return;
        }

/*****************************************************************************************************/
// Code below written to find active window by StackOverflow user "Jorge Ferreira" found at this link:
// https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c

        // Creates pointer to identify active window.
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        // Finds and returns active window.
        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

//End of stack overflow solution
/*****************************************************************************************************/

        /// <summary>
        /// Recieves direction to scroll then sends related keystokes to windows.
        /// </summary>
        /// <param name="direction">Int related to a keystoke or comination of keystokes to send</param>
        public static int SendKeystrokeToScroll(int direction)
        {
           // Creates process variable for excel.
            var excel = Process.GetProcessesByName("EXCEL").FirstOrDefault();
            var chrome = Process.GetProcessesByName("chrome").FirstOrDefault();

            // If Excel and Google Chrome are not currently open.
            if ((excel == null) && (chrome == null))
            {
                Console.WriteLine("Program Not Open");
                return -1;
            }

            // If neither Excel nor Google Chrome are the active windows.
            if ((GetActiveWindowTitle().Contains("excel") != true) && (GetActiveWindowTitle().Contains("chrome") != true))
            {
                Console.WriteLine("Excel and Chrome are the active window...");
                return -1;
            }

            Console.WriteLine("Scrolling...");

            // Chooses direction to scroll and sends keysrokes.
            switch (direction)
            {
                // Scroll down.
                case 1:
                    System.Windows.Forms.SendKeys.SendWait("{SCROLLLOCK}{DOWN}");
                    break;

                // Scroll up.
                case 2:
                    System.Windows.Forms.SendKeys.SendWait("{SCROLLLOCK}{Up}");
                    break;

                // Scroll left.
                case 3:
                    System.Windows.Forms.SendKeys.SendWait("{SCROLLLOCK}{LEFT}");
                    break;

                // Scroll right.
                case 4:
                    System.Windows.Forms.SendKeys.SendWait("{SCROLLLOCK}{RIGHT}");
                    break;
                default:
                    // Nothing to do here.
                    break;
            }
            return 1;
        }
    }
}

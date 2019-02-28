using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections;

namespace GestureSenseLibrary
{
    public class GestureLibrary
    {
        /*  Creates and returns serial port with default settings and a baud rate of 115200
         * 
         * */
        public static SerialPort AccessGS(string portName, int baud = 115200)
        {
            SerialPort port = new SerialPort(portName);
            port.BaudRate = baud;
            
            return port;
        }

        /*  Reades data from port and puts it into a queue
         * 
         * */
        public static Queue ReadPort(SerialPort port)
        {
            Queue myData = new Queue();
            //port.Open();
            try
            {
                if (port.IsOpen)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Console.Out.WriteLine("port is not open!!");
            }
            var b = port.ReadByte();

            //Console.WriteLine("Start");
            if ((int)b != 0)
            {
                while ((int)b != 255)
                {
                    //Console.WriteLine(b);
                    if ((int)b == 255 || ((int)b < 252 && (int)b > 0))
                    {
                        myData.Enqueue(b);
                    }
                    b = port.ReadByte();
                }
            }
            return myData;
        }

        /* Splits data from Queue into x values and z values
         * 
         * */
        public static void SortData(Queue myData, ref Queue xQueue, ref Queue zQueue)
        {
            int xMax = -1000;                   //Initializes to a value much smaller than possible on the sensor
            int xMin = -1000;                   //Initializes to a value much smaller than possible on the sensor
            int zMax = -1000;                   //Initializes to a value much smaller than possible on the sensor
            int zMin = -1000;                   //Initializes to a value much smaller than possible on the sensor
            int xRange = 0;
            int zRange = 0;


            if (myData.Count == 0)               //checks for empty queue
            {
                return;
            }

            while (myData.Count > 0)                                    //while queue is not empty
            {
                while (myData.Count > 0 && (int)myData.Peek() != 255)   //collect data group
                {
                    if ((int)myData.Peek() == 250)                      //if value in queue is 250, read x
                    {
                        myData.Dequeue();                               //dequeues message byte
                        if (xMax < (int)myData.Peek())
                        {
                            xMax = (int)myData.Peek();
                        }
                        else if (xMin > (int)myData.Peek())
                        {
                            xMin = (int)myData.Peek();
                        }
                        xQueue.Enqueue(myData.Dequeue());               //moves x value into x queue
                    }
                    else if ((int)myData.Peek() == 251)                 //if value in queue is 251, read z
                    {
                        myData.Dequeue();                               //dequeue message byte
                        if (zMax < (int)myData.Peek())
                        {
                            zMax = (int)myData.Peek();
                        }
                        else if (zMin > (int)myData.Peek())
                        {
                            zMin = (int)myData.Peek();
                        }
                        zQueue.Enqueue(myData.Dequeue());               //moves z value into z queue
                    }
                    else
                    {
                        myData.Dequeue();
                    }
                }
            }

            xRange = xMax - xMin;
            zRange = zMax - zMin - 15;

            return;
        }


        /* Processes data from queue into gestures
         * 
         * takes in queue and returns an int based off gesture
         * -1: no gesture
         * 1: value increased (right swipe)
         * 2: value decreased (left swipe)
         * 3: value increased then decreased (left bump)
         * 4: value decreased then increased (right bump)
         * */
        public static int Gesture(Queue xQueue)
        {
            if (xQueue.Count == 0) //Checks if queue is empty
            {
                return -1;
            }
            int sum = 0;
            int traverse = 0;
            int xMax = (int)xQueue.Peek();
            int xMin = (int)xQueue.Peek();
            traverse = xMax;
            int previous = traverse;
            int firstVal = traverse;
            int secondVal = traverse;
            int lastVal = 0;
            int total = 0;
            int counter = 0;
            int numVals = xQueue.Count;
            
            while (xQueue.Count > 0)
            {
                if (counter == 0) //removes first item
                {
                    xQueue.Dequeue();
                    if (xQueue.Count != 0 && xQueue.Count > 3)
                    {
                        //Resetting first item
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

                //find max and min
                if (traverse > xMax)
                {
                    xMax = traverse;
                }
                else if (traverse < xMin)
                {
                    xMin = traverse;
                }

                if (traverse >= previous)                //add if value is increasing
                {
                    sum += traverse;
                    total += traverse;
                }
                else
                {                                //subtract if value is decreasing
                    sum -= traverse;
                    total -= traverse;
                }

                previous = traverse;                    //stores previous value
                xQueue.Dequeue();                       //removes current values from queue
                if (xQueue.Count != 0)                  //if queue isn't empty
                    traverse = (int)xQueue.Peek();      //updates current value

                counter++;
                if (counter == 1)                       //if second value in queue
                    secondVal = traverse;
            }
            lastVal = previous;


            if (firstVal < xMax && secondVal < xMax && lastVal < xMax)          //if the first value (and !OR! second value for redundency) is less than the max then hand entered and exited from left so BUMP_LEFT
            {
                Console.WriteLine("BUMP_LEFT");

                Console.WriteLine("*******************");
                return 3;
            }
            else if (sum > xMax)                              //it is right swipe
            {
                Console.WriteLine("RIGHT_SWIPE");

                Console.WriteLine("*******************");
                return 1;
            }
            else if (/*firstVal > xMin && */secondVal > xMin && lastVal > xMin) //if the first value (or second value for redundency) is greater than the min then hand entered and exited from right so BUMP_RIGHT
            {
                Console.WriteLine("BUMP_RIGHT");

                Console.WriteLine("*******************");
                return 4;
            }
            else if (sum < xMin)                              //it is left swipe
            {
                Console.WriteLine("LEFT_SWIPE");

                Console.WriteLine("*******************");
                return 2;
            }
            return -1;
        }


        public static void GestureSetter(int gestureCode)
        {
            switch (gestureCode)
            {
                case 1:                                                     //right swipe
                    System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
                    break;
                case 2:                                                     //left swipe
                    System.Windows.Forms.SendKeys.SendWait("{LEFT}");
                    break;
                case 3:                                                     //right bump
                    System.Windows.Forms.SendKeys.SendWait("{LEFT 2}");
                    break;
                case 4:                                                     //left bump
                    System.Windows.Forms.SendKeys.SendWait("{RIGHT 2}");
                    break;
                //case 5:
                //    System.Windows.Forms.SendKeys.SendWait("^({F5})");
                //    break;
                //case 6:
                //    System.Windows.Forms.SendKeys.SendWait("{ESC}");
                //    break;
                default:
                    Console.WriteLine("Error: No Gesture");
                    break;
            }
        }
    }
}

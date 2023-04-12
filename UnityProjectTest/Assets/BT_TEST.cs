/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

// This code is modified from the original Arditty Source Code

using UnityEngine;
using System.Threading;
using System.Globalization;

/**
 * This class allows a Unity program to continually check for messages from a
 * serial device.
 *
 * It creates a Thread that communicates with the serial port and continually
 * polls the messages on the wire.
 * That Thread puts all the messages inside a Queue, and this SerialController
 * class polls that queue by means of invoking SerialThread.GetSerialMessage().
 *
 * The serial device must send its messages separated by a newline character.
 * Neither the SerialController nor the SerialThread perform any validation
 * on the integrity of the message. It's up to the one that makes sense of the
 * data.
 */
public class BT_TEST : MonoBehaviour
{
    const int X = 0;
    const int Z = 1;
    const int Y = 2;
    const int W = 3;
    const int DRAW = 4;
    const int MENU = 5;
    const int CALIBRATE = 6;

    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string xyPortName = "COM3";
    public string zPortName = "COM3";
    public string rotationPortName = "COM3";

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 9600;

    [Tooltip("Reference to an scene object that will receive the events of connection, " +
             "disconnection and the messages from the serial device.")]
    public GameObject messageListener;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 1;

    public string xyMessage = "0.0,0.0";
    public string zMessage = "0.0";
    public string penMessage = "0.0,0.0,0.0,0.0,0,0,0";

    public GameObject tip;
    public GameObject ink;

    // Constants used to mark the start and end of a connection. There is no
    // way you can generate clashing messages from your serial device, as I
    // compare the references of these strings, no their contents. So if you
    // send these same strings from the serial device, upon reconstruction they
    // will have different reference ids.
    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

    // Internal reference to the Thread and the object that runs in it.
    protected Thread xyThread;
    protected SerialThreadLines xySerialThread;

    protected Thread zThread;
    protected SerialThreadLines zSerialThread;

    protected Thread penThread;
    protected SerialThreadLines penSerialThread;

    private Quaternion offsetQuat = new Quaternion(0, 0, 0, 1);
    private bool resetFlag = false;
    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void OnEnable()
    {
        xySerialThread = new SerialThreadLines(xyPortName,
                                             baudRate,
                                             reconnectionDelay,
                                             maxUnreadMessages);

        zSerialThread = new SerialThreadLines(zPortName,
                                             baudRate,
                                             reconnectionDelay,
                                             maxUnreadMessages);

        penSerialThread = new SerialThreadLines(rotationPortName,
                                             baudRate,
                                             reconnectionDelay,
                                             maxUnreadMessages);

        
        xyThread = new Thread(new ThreadStart(xySerialThread.RunForever));
        xyThread.Start();

        zThread = new Thread(new ThreadStart(zSerialThread.RunForever));
        zThread.Start();

        penThread = new Thread(new ThreadStart(penSerialThread.RunForever));
        penThread.Start();
        
    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    void OnDisable()
    {
        // If there is a user-defined tear-down function, execute it before
        // closing the underlying COM port.
        if (userDefinedTearDownFunction != null)
            userDefinedTearDownFunction();

        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (xySerialThread != null)
        {
            xySerialThread.RequestStop();
            xySerialThread = null;
        }

        if (zSerialThread != null)
        {
            zSerialThread.RequestStop();
            zSerialThread = null;
        }

        if (penSerialThread != null)
        {
            penSerialThread.RequestStop();
            penSerialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (xyThread != null)
        {
            xyThread.Join();
            xyThread = null;
        }

        
        if (zThread != null)
        {
            zThread.Join();
            zThread = null;
        }
        

        if (penThread != null)
        {
            penThread.Join();
            penThread = null;
        }
    }

    // ------------------------------------------------------------------------
    // Polls messages from the queue that the SerialThread object keeps. Once a
    // message has been polled it is removed from the queue. There are some
    // special messages that mark the start/end of the communication with the
    // device.
    // ------------------------------------------------------------------------
    void Update()
    {
        // If the user prefers to poll the messages instead of receiving them
        // via SendMessage, then the message listener should be null.
        if (messageListener == null)
            return;

        // Read the next message from the queue
        string temp1 = (string)xySerialThread.ReadMessage();
        string temp2 = (string)zSerialThread.ReadMessage();
        string temp3 = (string)penSerialThread.ReadMessage();

        if (temp1 != null)
            xyMessage = temp1;
        if (temp2 != null)
            zMessage = temp2;
        if (temp3 != null)
            penMessage = temp3;

        string[] xy = xyMessage.Split(',');
        zMessage = zMessage.Trim();
        string[] pen = penMessage.Split(',');
     
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        float x = 0f,y = 0f,z = 0f;
        //Vector3 currentAngle = new Vector3(0,0,0);
        Quaternion currentQuat = new Quaternion(0, 0, 0, 0);
        if(xy.Length == 2)
        {
            float.TryParse(xy[0], NumberStyles.Any, ci, out x);
            float.TryParse(xy[1], NumberStyles.Any, ci, out y);
        }
        float.TryParse(zMessage, NumberStyles.Any, ci, out z);
        //if (float.TryParse(rotations[0], NumberStyles.Any, ci, out currentAngle.x))
        //if (float.TryParse(rotations[1], NumberStyles.Any, ci, out currentAngle.y))
        //if (float.TryParse(rotations[2], NumberStyles.Any, ci, out currentAngle.z))
        
        Quaternion frontQuat = new Quaternion(0, 0, 0, 1);
        // Config that works only when the pen is piointed downwards -y, x, -z
        if (pen.Length == 7)
        {
            float.TryParse(pen[X], NumberStyles.Any, ci, out currentQuat.x);
            float.TryParse(pen[Z], NumberStyles.Any, ci, out currentQuat.z);
            float.TryParse(pen[Y], NumberStyles.Any, ci, out currentQuat.y);
            float.TryParse(pen[W], NumberStyles.Any, ci, out currentQuat.w);
            currentQuat.y = -currentQuat.y;
            if (pen[DRAW].Equals("0"))
                Draw();
            if (pen[MENU].Equals("0"))
                Menu();
            if (pen[CALIBRATE].Equals("0"))
            {
                //if(resetFlag)
               // {
                    //offsetQuat = new Quaternion(0, 0, 0, 1);
                   // resetFlag = false;

                //}
               // else
              //  {
                    offsetQuat = Quaternion.Inverse(currentQuat) * frontQuat;
                    offsetQuat.x = 0;
                    offsetQuat.z = 0;
                    resetFlag = true;
                //}
                
            }
        }
        transform.position = new Vector3(x,1 + y*-1,z); // (-5 + x * 10,5 + y * -10,-5 + z * 10);
        //transform.eulerAngles = currentAngle;
        //currentQuat = currentQuat * offsetVal;
        //Quaternion offsetQuat = new Quaternion(0, 0, 0, 0);
        //currentQuat.x = -currentQuat.x;
        //currentQuat.z = -currentQuat.z;
        currentQuat.y = -currentQuat.y;
        transform.rotation = currentQuat * offsetQuat;
    }

    public void Draw()
    {
        Instantiate(ink, tip.transform.position, tip.transform.rotation);
    }

    public void Menu()
    {

    }

    public void Calibrate()
    {

    }
    
    // ------------------------------------------------------------------------
    // Executes a user-defined function before Unity closes the COM port, so
    // the user can send some tear-down message to the hardware reliably.
    // ------------------------------------------------------------------------
    public delegate void TearDownFunction();
    private TearDownFunction userDefinedTearDownFunction;
    public void SetTearDownFunction(TearDownFunction userFunction)
    {
        this.userDefinedTearDownFunction = userFunction;
    }

}

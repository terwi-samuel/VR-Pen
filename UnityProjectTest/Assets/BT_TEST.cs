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
    public string rotationMessage = "0.0,0.0,0.0,0.0";

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

    protected Thread rotationThread;
    protected SerialThreadLines rotationSerialThread;


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

        rotationSerialThread = new SerialThreadLines(rotationPortName,
                                             baudRate,
                                             reconnectionDelay,
                                             maxUnreadMessages);

        
        xyThread = new Thread(new ThreadStart(xySerialThread.RunForever));
        xyThread.Start();

        zThread = new Thread(new ThreadStart(zSerialThread.RunForever));
        zThread.Start();

        rotationThread = new Thread(new ThreadStart(rotationSerialThread.RunForever));
        rotationThread.Start();
        
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

        if (rotationSerialThread != null)
        {
            rotationSerialThread.RequestStop();
            rotationSerialThread = null;
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
        

        if (rotationThread != null)
        {
            rotationThread.Join();
            rotationThread = null;
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
        string temp3 = (string)rotationSerialThread.ReadMessage();

        if (temp1 != null)
            xyMessage = temp1;
        if (temp2 != null)
            zMessage = temp2;
        if (temp3 != null)
            rotationMessage = temp3;

        string[] xy = xyMessage.Split(',');
        zMessage = zMessage.Trim();
        string[] rotations = rotationMessage.Split(',');

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        float x = 0f,y = 0f,z = 0f;
        //Vector3 currentAngle = new Vector3(0,0,0);
        Quaternion currentQuat = new Quaternion(0, 0, 0, 0);
        if(xy.Length == 2)
        {
            if (float.TryParse(xy[0], NumberStyles.Any, ci, out x));
            if (float.TryParse(xy[1], NumberStyles.Any, ci, out y));
        }
        if (float.TryParse(zMessage, NumberStyles.Any, ci, out z));
        //if (float.TryParse(rotations[0], NumberStyles.Any, ci, out currentAngle.x))
        //if (float.TryParse(rotations[1], NumberStyles.Any, ci, out currentAngle.y))
        //if (float.TryParse(rotations[2], NumberStyles.Any, ci, out currentAngle.z))
        if(rotations.Length == 4)
        {
            if (float.TryParse(rotations[0], NumberStyles.Any, ci, out currentQuat.x));
            if (float.TryParse(rotations[1], NumberStyles.Any, ci, out currentQuat.y));
            if (float.TryParse(rotations[2], NumberStyles.Any, ci, out currentQuat.z));
            if (float.TryParse(rotations[3], NumberStyles.Any, ci, out currentQuat.w));
        }
        
        transform.position = new Vector3(-5 + x * 10,5 + y * -10,-5 + z * 10);
        //transform.eulerAngles = currentAngle;
        transform.rotation = currentQuat;
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

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
public class Bluetooth : MonoBehaviour
{
    // Array access constants
    const int X = 0;
    const int Z = 1;
    const int Y = 2;
    const int W = 3;
    const int DRAW = 6;
    const int MENU = 5;
    const int CALIBRATE = 4;

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

    // Global strings for use outside script
    public string xyMessage = "0.0,0.0";
    public string zMessage = "0.0";
    public string penMessage = "0.0,0.0,0.0,0.0,0,0,0";

    public GameObject textures;

    // GameObjects to link pen model
    public GameObject tip;
    public GameObject ink;

    // Constants used to mark the start and end of a connection. There is no
    // way you can generate clashing messages from your serial device, as I
    // compare the references of these strings, no their contents. So if you
    // send these same strings from the serial device, upon reconstruction they
    // will have different reference ids.
    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

    // Internal reference to the Threads and the objects that runs in it.
    protected Thread xyThread;
    protected SerialThreadLines xySerialThread;

    protected Thread zThread;
    protected SerialThreadLines zSerialThread;

    protected Thread penThread;
    protected SerialThreadLines penSerialThread;

    private Quaternion offsetQuat = new Quaternion(0, 0, 0, 1);
    private CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
    private bool heldDown;
    private Renderer sphereRenderer;
    private Renderer cubeRenderer;
    private Renderer capsuleRenderer;
    //private bool resetFlag = false;
    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void OnEnable()
    {
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

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

    void Start()
    {
        sphereRenderer = ink.transform.GetChild(0).gameObject.GetComponent<Renderer>();
        cubeRenderer = ink.transform.GetChild(1).gameObject.GetComponent<Renderer>();
        capsuleRenderer = ink.transform.GetChild(2).gameObject.GetComponent<Renderer>();
    }

    // ------------------------------------------------------------------------
    // Polls messages from the queue that the SerialThread object keeps. Once a
    // message has been polled it is removed from the queue. There are some
    // special messages that mark the start/end of the communication with the
    // device.
    // ------------------------------------------------------------------------
    void Update()
    {
        // Variable initilization
        float x = 0f, y = 0f, z = 0f;
        Quaternion currentQuat = new Quaternion(0, 0, 0, 0);
        Quaternion frontQuat = new Quaternion(0, 0, 0, 1);

        // If the user prefers to poll the messages instead of receiving them
        // via SendMessage, then the message listener should be null.
        if (messageListener == null)
            return;

        // Read the next message from the queue for each bluetooth device
        string[] buffer = readQueue();

        // If the message we recieved is not null then we update the message
        if (buffer[0] != null)
            xyMessage = buffer[0];
        if (buffer[1] != null)
            zMessage = buffer[1];
        if (buffer[2] != null)
            penMessage = buffer[2];

        // Extract the messages and put them into a single struct
        var messages = extractMessage();

        // We must make sure both values were sent via the bluetooth connection
        if (messages.xy.Length == 2)
        {
            x = parseFloat(messages.xy[0]);
            y = parseFloat(messages.xy[1]);
        }
        z = parseFloat(messages.z);

        // We must make sure all 7 values were sent by the pen
        if (messages.pen.Length == 7)
        {
            // We must parse the floats for the quaternians.
            // Note: We are using our X Y Z W constants which swap the Y and Z axis
            // and we are negating the Y axis as well. This is to convert from the IMU's
            // Right handed system to Unitys left handed system.
            currentQuat.x = parseFloat(messages.pen[X]);
            currentQuat.y = -parseFloat(messages.pen[Y]);
            currentQuat.z = parseFloat(messages.pen[Z]);
            currentQuat.w = parseFloat(messages.pen[W]);
            // Here we are getting the button presses where 0 represents pressed.
            if (messages.pen[DRAW].Equals("0"))
                Draw();
            if (messages.pen[MENU].Equals("0"))
            {
                if (!heldDown)
                    Menu();
                heldDown = true;
            }
            else
                heldDown = false;
                
            if (messages.pen[CALIBRATE].Equals("0"))
                Calibrate(currentQuat, frontQuat);
        }
        // Convert all the data we extracted into transforms to be seen in the viewport
        transform.position = new Vector3(x - 0.5f, (float)1.5 + -y, z -0.5f);
        transform.rotation = offsetQuat * currentQuat;
    }

    public void Draw()
    {
        // Create "ink" spheres at the given tip object
        var obj = Instantiate(ink, tip.transform.position, tip.transform.rotation);
        obj.SetActive(true);
    }


    // update color creates a renderer for each brush type, places the color input into the switch
    // statement and then sets the color of all brush types to the selected one
    public void updateColor(string color)
    {
        Color customColor = new Color();

        switch (color)
        {
            case "black":
                customColor = new Color(0f, 0f, 0f, 1f);
                break;
            case "gray":
                customColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                break;
            case "white":
                customColor = new Color(1f, 1f, 1f, 1f);
                break;
        }
        sphereRenderer.material.color = customColor;
        cubeRenderer.material.color = customColor;
        capsuleRenderer.material.color = customColor;
    }

    // update size updates the size of the ink object to the selected size
    public void updateSize(float size)
    {
        ink.transform.localScale = new Vector3(size,size,size);
    }


    // update texture first disables all the brush types then enables the selected one
    public void updateTexture(int selection)
    {
        ink.transform.GetChild(0).gameObject.SetActive(false);
        ink.transform.GetChild(1).gameObject.SetActive(false);
        ink.transform.GetChild(2).gameObject.SetActive(false);

        ink.transform.GetChild(selection).gameObject.SetActive(true);
    }

    
    // Menu function just enables and disables the menu
    public void Menu()
    {
        if (textures.activeInHierarchy)
            textures.SetActive(false);
        else
            textures.SetActive(true);
    }

    public void Calibrate(Quaternion currentQuat, Quaternion frontQuat)
    {
        // Offset the quaternion by the difference between its orientation and the Z axis.
        offsetQuat = Quaternion.Inverse(currentQuat) * frontQuat;
        offsetQuat.x = 0;
        offsetQuat.z = 0;
    }

    // ----------------------------------------------------------------------------
    // Reads the messages from each thread and stores it into an array of strings
    // ----------------------------------------------------------------------------
    public string[] readQueue()
    {
        string temp1 = (string)xySerialThread.ReadMessage();
        string temp2 = (string)zSerialThread.ReadMessage();
        string temp3 = (string)penSerialThread.ReadMessage();

        string[] buffer = { temp1, temp2, temp3 };
        return buffer;
    }

    // ----------------------------------------------------------
    // This takes each message, cleans it up, and returns it as 
    // a struct to keep all variables nice and together.
    // ----------------------------------------------------------
    public (string[] xy, string z, string[] pen) extractMessage()
    {
        string[] xySplit = xyMessage.Trim().Split(',');
        string zTrim = zMessage.Trim();
        string[] penSplit = penMessage.Trim().Split(',');

        return (xySplit, zTrim, penSplit);
    }

    // ----------------------------------------------------------
    // This just TryParses the given string for a float
    // ----------------------------------------------------------
    public float parseFloat(string input)
    {
        float output = 0f;
        float.TryParse(input, NumberStyles.Any, ci, out output);
        return output;
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

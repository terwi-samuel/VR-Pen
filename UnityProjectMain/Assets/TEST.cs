using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class TEST : MonoBehaviour
{
    SerialPort stream;
    // Start is called before the first frame update
    void Start()
    {
        stream = new SerialPort("COM4", 19200);
        stream.ReadTimeout = 50;
        stream.Open();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(
            AsynchronousReadFromArduino
            ((string s) => Debug.Log(s),     // Callback
            () => Debug.LogError("Error!"),  // Error callback
            10000f                           // Timeout (milliseconds)
        ));
    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield break; // Terminates the Coroutine
            }
            else
                yield return null; // Wait for next frame

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }


}

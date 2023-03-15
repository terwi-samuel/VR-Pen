using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TEXT : MonoBehaviour
{
    public Text textElement;
    public BT_TEST bt;
    // Start is called before the first frame update
    void Start()
    {
        textElement.text = "Hello World!";
    }

    // Update is called once per frame
    void Update()
    {
        string message = bt.xyMessage + "\n" + bt.zMessage + "\n" + bt.rotationMessage;
        textElement.text = message;
    }
}

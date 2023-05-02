using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class scr : MonoBehaviour
{
    public Text myText;
    public GameObject test;

    void Update()
    {
        myText.text = test.ToString();
    }
}


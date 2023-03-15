using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DELET : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string str = "1.00000,1.33337";
        string[] arr = str.Split(',');
        Debug.Log("Index 0: " + arr[0]);
        Debug.Log("Index 1: " + arr[1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

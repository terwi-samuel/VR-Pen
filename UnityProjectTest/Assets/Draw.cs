using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour
{
    public GameObject tip;
    public GameObject ink;
    public string text;
    // Start is called before the first frame update
    void Start()
    {
        text = "Hellow Word";
    }

    void Ink()
    {
        Instantiate(ink, tip.transform.position, tip.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            Instantiate(ink,tip.transform.position,tip.transform.rotation);
            text = "SPACE IS DOWN";
        }
        else
        {
            text = "SPACE IS UP";
        }
    }
}

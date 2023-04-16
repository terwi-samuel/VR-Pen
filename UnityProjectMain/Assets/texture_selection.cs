using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class texture_selection : MonoBehaviour
{
    public GameObject textures;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (textures.activeInHierarchy)
            {
                textures.SetActive(false);
            } 
            else
            {
                textures.SetActive(true);
            }
        }
    }
}
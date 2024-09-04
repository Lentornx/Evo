using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldScript : MonoBehaviour
{
    public Vector3 wind = new Vector3(0, 0, 0);
    void Start()
    {
        InvokeRepeating("WindChange", 0, 10);
    }

    void WindChange()
    {
        
        wind = new Vector3(
            Random.Range(-1f, 1f), 
            -0.2f, // ta wartosc bedzie w przyszlosci zalezna od genow nasionka
            Random.Range(-1f, 1f)   
        );
        //Debug.Log("Wind changed to: " + wind);
    }
}

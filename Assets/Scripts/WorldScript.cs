using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Vector3 currentWind;
    public Vector3 targetWind;
    public float transitionSpeed = 1f; // Speed of transition
    public int targetFrameRate = 120;
    public bool limitFPS = true;
    void Start()
    {
        InvokeRepeating("WindChange", 0, 10);
        if (limitFPS)
            Application.targetFrameRate = targetFrameRate;
    }

    void Update()
    {
        currentWind = Vector3.Lerp(currentWind, targetWind, Time.deltaTime * transitionSpeed);
    }

    void WindChange()
    {
        targetWind = new Vector3(
            Random.Range(-1f, 1f), 
            -0.2f, // ta wartosc bedzie w przyszlosci zalezna od genow nasionka
            Random.Range(-1f, 1f)   
        );
        //Debug.Log("Wind changed to: " + targetWind);
    }
}

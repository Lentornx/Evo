using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedScript : MonoBehaviour
{
    private WorldScript world;
    public float movementSpeed = 1.0f; // bedzie sparametryzowane i zale¿ne od genow
    public GameObject plant;
    private bool hasSpawnedPlant = false;  
    // later on the seed will have all the genes of the plant it will spawn

    void Start()
    {
        if (world == null)
        {
            world = FindObjectOfType<WorldScript>(); // do pobiierania wartosci wiatru
        }
    }

    void Update()
    {
        transform.position += world.wind * movementSpeed * Time.deltaTime;
        if(transform.position.y < -3) // na wypadek wylecenia poza mapê
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasSpawnedPlant && other.gameObject.GetComponent<GroundSegment>() != null)
        {
            Transform plantsParent = GameObject.Find("Plants").transform;
            hasSpawnedPlant = true; // w innym przypadku umie zespawnic kilka drzew, bo skoliduje z kilkoma fragmentami naraz
            Instantiate(plant, transform.position, Quaternion.identity, plantsParent);
            Destroy(gameObject);
        }
    }
}

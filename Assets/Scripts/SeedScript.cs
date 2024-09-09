using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    private World world;
    public float movementSpeed = 1.0f; // bedzie sparametryzowane i zale¿ne od genow
    public GameObject plant;
    private bool hasSpawnedPlant = false;
    // later on the seed will have all the genes of the plant it will spawn

    public float baseNutrientConsumption = 1.0f;
    public float growthSurplusThreshold = 0.05f;
    public int heightCeiling = 20;
    public float metabolism = 1.0f;
    public float seedCost = 0.1f;
    public float startingGrowth = 0.1f;
    private Vector3 bias; // po to by nasiona porusza³y siê nieco bardziej losowo
    public GameObject plantsParent;
    private PlantPool plantPool;

    void Start()
    {
        InvokeRepeating("changeBias", 0, 1.0f);
        if (world == null)
        {
            world = FindObjectOfType<World>(); // do pobiierania wartosci wiatru
        }
        plantPool = FindObjectOfType<PlantPool>();
}

    void changeBias()
    {
        bias = new Vector3(
            Random.Range(-0.1f, 0.1f),
            -0.2f, // ta wartosc bedzie w przyszlosci zalezna od genow nasionka
            Random.Range(-0.1f, 0.1f)
        );
    }

    void Update()
    {
        transform.position += (world.currentWind + bias) * movementSpeed * Time.deltaTime;
        if (transform.position.y < -3) // na wypadek wylecenia poza mapê
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasSpawnedPlant && other.gameObject.GetComponent<GroundSegment>() != null)
        {
            hasSpawnedPlant = true; // w innym przypadku umie zespawnic kilka drzew w jednym miejscu, bo skoliduje z kilkoma fragmentami naraz
            GameObject newObject = plantPool.GetPlant(transform.position, Quaternion.identity);
            Plant newPlant = newObject.GetComponent<Plant>();
            Seed s = gameObject.GetComponent<Seed>();
            newPlant.SetupVariables(s.baseNutrientConsumption, s.growthSurplusThreshold, s.heightCeiling, s.metabolism, seedCost, startingGrowth);
            Destroy(gameObject);
        }
    }
    public void SetupVariables(float v1, float v2, int v3, float v4, float v5, float v6)
    {
        baseNutrientConsumption = v1;
        growthSurplusThreshold = v2;
        heightCeiling = v3;
        metabolism = v4;
        seedCost = v5;
        startingGrowth = v6;
    }

}
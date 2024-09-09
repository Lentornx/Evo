using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSegment : MonoBehaviour
{
    public float nutrients = 100.0f;
    public float maxNutrients = 100.0f;
    public float replenishmentRate = 1.0f;
    public float replenishmentInterval = 4.0f;
    public float threshold = 1.0f;
    public float decayMultiplier = 2.0f;
    public bool dynamicMat = false;

    public Material fullMaterial;

    private Renderer rend;
    private Dictionary<Plant, float> extractingPlants = new Dictionary<Plant, float>(); //contains distance from this segment

    void Start()
    {
        rend = GetComponent<Renderer>(); // renderuje nam zmienjacy sie kolor
        InvokeRepeating("ReplenishNutrients", replenishmentInterval, replenishmentInterval);
    }

    public void StartExtracting(Plant p)
    {
        Vector3 plantPositionXZ = new Vector3(p.transform.position.x, 0, p.transform.position.z);
        float squaredDistance = (transform.position - plantPositionXZ).sqrMagnitude;
        // liczymy dystans od bazy rosliny (gdzie siê zaczynaj¹ korzenie)
        extractingPlants.TryAdd(p, squaredDistance);
    }

    public void StopExtracting(Plant plant)
    {
        extractingPlants.Remove(plant);
    }

    public float ExtractNutrients(Plant plant, float amount)
    // idea, kazdy segment bedzie wiedzial w ilu procentach jest zakorzeniony wzglêdem kazdej rosliny, a inkrement zakorzenienia bêdzie zale¿a³ od statów i odleg³osci rosliny
    {
        float nutrientsGiven = 0.0f;
        float totalDemand = 0.0f; // how much total every plant wants from this segment

        float squaredRange = (plant.range * plant.height) * (plant.range * plant.height);
        float currProximityFactor = Mathf.Clamp01(1.0f - (extractingPlants[plant] / squaredRange));

        foreach (var p in extractingPlants)
        {
            float proximityFactor = Mathf.Clamp01(1.0f - (p.Value / ((p.Key.range * p.Key.height)* (p.Key.range * p.Key.height)) )); // jezeli size sie nie zmieni to mozna zapisac w dictionary innym
            totalDemand += p.Key.adjustedNutrientConsumption / p.Key.consumeInterval * proximityFactor;
            //mo¿liwe ¿e tutaj dajemy baseNutrientConsumption, by symulowaæ ¿e s¹ tam korzenie, a nie ile faktycznie pobieraj¹
        }

        // If total demand exceeds a threshold, distribute proportionally based on proximity and extraction speed
        if (totalDemand > threshold)
        {
            float proportion = (amount / plant.consumeInterval * currProximityFactor) / totalDemand;
            nutrientsGiven = Mathf.Min(threshold * proportion, nutrients);
        }
        else
        {
            nutrientsGiven = Mathf.Min(amount * currProximityFactor, nutrients);
        }

        nutrients -= nutrientsGiven;
        if(dynamicMat == true)
            UpdateColor();

        return nutrientsGiven;
    }

    public bool HasNutrients()
    {
        return nutrients > 0;
    }

    private void UpdateColor()
    {
        float nutrientRatio = nutrients / maxNutrients;
        rend.material.color = Color.Lerp(Color.black, fullMaterial.color, nutrientRatio); // im mniej nutrientow wzgledem maksymalnej ilosci tym bardziej szary kolor 
    }

    private void ReplenishNutrients()
    {
        nutrients += replenishmentRate;
        if (nutrients > maxNutrients)
        {
            nutrients = maxNutrients;
        }
    }

    public void GetNutrientsFromDecay(Plant p)
    {
        Vector3 plantPositionXZ = new Vector3(p.transform.position.x, 0, p.transform.position.z); 
        float squaredDistance = (transform.position - plantPositionXZ).sqrMagnitude;
   
        float squaredRange = (p.range * p.height) * (p.range * p.height);

        float proximityFactor = Mathf.Clamp01(1.0f - (squaredDistance / squaredRange));
        nutrients = Mathf.Min(maxNutrients, nutrients + (proximityFactor * p.height) / decayMultiplier); //wspó³czynnik rozkladu do sparametryzowania
    }
}
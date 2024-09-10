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

    public struct PlantInfo
    {
        public float squaredDistance;
        public float proximityFactor;

        public PlantInfo(float squaredDistance, float proximityFactor)
        {
            this.squaredDistance = squaredDistance;
            this.proximityFactor = proximityFactor;
        }
    }

    private Dictionary<Plant, PlantInfo> extractingPlants = new Dictionary<Plant, PlantInfo>(); //contains distance from this segment

    void Start()
    {
        rend = GetComponent<Renderer>(); // renderuje nam zmienjacy sie kolor
        InvokeRepeating("ReplenishNutrients", replenishmentInterval, replenishmentInterval);
    }

    public void StartExtracting(Plant p)
    {
        float proximityFactor;
        PlantInfo plantInfo;
        float squaredRange = (p.range * p.height) * (p.range * p.height); //to zawsze sprawdzamy bo ta funkcja odpala siê tylko przy zmianie height
        // to do - plants powinny miec vara current range i tam co growth zmieniac, zamiast ciagle przemnazac tu i w inyych funkcjach
        bool isPlantPresent = extractingPlants.ContainsKey(p);
        if (!isPlantPresent)
        {
            Vector3 plantPositionXZ = new Vector3(p.transform.position.x, 0, p.transform.position.z);
            float squaredDistance = (transform.position - plantPositionXZ).sqrMagnitude;
            proximityFactor = Mathf.Clamp01(1.0f - (squaredDistance / squaredRange));
            PlantInfo newInfo = new PlantInfo(squaredDistance, proximityFactor);
            plantInfo = new PlantInfo(squaredDistance, proximityFactor);
            extractingPlants.Add(p, plantInfo);
        }
        else
        {
            proximityFactor = Mathf.Clamp01(1.0f - (extractingPlants[p].squaredDistance / squaredRange));
            plantInfo = new PlantInfo(extractingPlants[p].squaredDistance, proximityFactor);
            extractingPlants[p] = plantInfo;
        }       
    }

    public void StopExtracting(Plant plant)
    {
        extractingPlants.Remove(plant);
    }

    public float ExtractNutrients(Plant plant, float amount)
    {
        float nutrientsGiven = 0.0f;
        float totalDemand = 0.0f; // how much in total is wanted from this segment

        float squaredRange = (plant.range * plant.height) * (plant.range * plant.height);
        float currProximityFactor = extractingPlants[plant].proximityFactor;

        foreach (var p in extractingPlants)
        {
            totalDemand += p.Key.adjustedNutrientConsumption / p.Key.consumeInterval * p.Value.proximityFactor;
            // mo¿liwe ¿e tutaj damy baseNutrientConsumption zamiast adjusted, by symulowaæ ¿e s¹ tam korzenie ograniczaj¹ce pobieranie innych roœlin
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
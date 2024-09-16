using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GroundSegment : MonoBehaviour
{
    public float nutrients = 100.0f;
    public float maxNutrients = 100.0f;
    public float replenishmentRate = 1.0f;
    public float threshold = 3.0f;
    public float decayMultiplier = 2.0f;
    public float finalDemand = 0.0f;

    private Dictionary<Plant, float> Demand = new Dictionary<Plant, float>();

    public Material fullMaterial;
    private Renderer rend;
    public bool dynamicMat = false;

    void Start()
    {
        rend = GetComponent<Renderer>(); // renderuje nam zmienjacy sie kolor
    }

    public void addToExtraction(Plant plant, float consumption)
    {
        Demand.TryAdd(plant, consumption);
        finalDemand += consumption;
    }

    public void RemoveFromExtraction(Plant plant)
    {
        finalDemand -= Demand[plant];
        Demand.Remove(plant);
    }

    public float ExtractNutrients(Plant plant, float amount, float interval)
    {
        float currentDemand = amount / interval;
        finalDemand -= Demand[plant];
        Demand[plant] = currentDemand;
        finalDemand += currentDemand;

        if(threshold < finalDemand)
        {
            amount = currentDemand / finalDemand * threshold;
        }

        amount = Mathf.Min(amount, nutrients);
        nutrients -= amount;
        if(dynamicMat == true)
            UpdateColor();

        return amount;
    }

    public bool HasNutrients()
    {
        return nutrients > 0;
    }

    private void UpdateColor()
    {
        float nutrientRatio = nutrients / maxNutrients;
        rend.material.color = Color.Lerp(Color.black, fullMaterial.color, nutrientRatio); // im mniej nutrientow wzgledem maksymalnej ilosci tym bardziej czarny kolor 
    }

    public void ReplenishNutrients()
    {
        nutrients += replenishmentRate;
        if (nutrients > maxNutrients)
        {
            nutrients = maxNutrients;
        }
    }

    public void ReplenishNutrients(float amount)
    {
        nutrients += amount;
        if (nutrients > maxNutrients)
        {
            nutrients = maxNutrients;
        }
    }
}
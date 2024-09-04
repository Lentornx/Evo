using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSegmentBASIC : MonoBehaviour
{
    public float nutrients = 100.0f;
    public float maxNutrients = 100.0f;
    public float replenishmentRate = 1.0f;
    public float replenishmentInterval = 4.0f;

    private Renderer rend;
    private HashSet<Plant> extractingPlants = new HashSet<Plant>();

    void Start()
    {
        rend = GetComponent<Renderer>(); // renderuje nam zmienjacy sie kolor
        UpdateColor();
        InvokeRepeating("ReplenishNutrients", replenishmentInterval, replenishmentInterval);
    }

    public void StartExtracting(Plant plant)
    {
        extractingPlants.Add(plant);
    }

    public void StopExtracting(Plant plant)
    {
        extractingPlants.Remove(plant);
    }

    public float ExtractNutrients(Plant plant, float amount)
    // idea, kazdy segment bedzie wiedzial w ilu procentach jest zakorzeniony względem kazdej rosliny, a inkrement zakorzenienia będzie zależał od statów i odległosci rosliny
    {
        float nutrientsGiven = 0.0f;
        if (extractingPlants.Contains(plant))
        {
            float sharedAmount = amount / extractingPlants.Count; // in the future the share will be affected by size and/or distance of every plant from the segment
            nutrientsGiven = Mathf.Min(sharedAmount, nutrients);
            nutrients -= nutrientsGiven;
            UpdateColor();
        }
        return nutrientsGiven;
    }

    public bool HasNutrients()
    {
        return nutrients > 0;
    }

    void UpdateColor()
    {
        float nutrientRatio = nutrients / maxNutrients;
        rend.material.color = Color.Lerp(Color.black, Color.green, nutrientRatio); // im mniej nutrientow wzgledem maksymalnej ilosci tym bardziej szary kolor
    }

    void ReplenishNutrients()
    {
        nutrients += replenishmentRate;
        if (nutrients > maxNutrients)
        {
            nutrients = maxNutrients;
        }
        UpdateColor();
    }
}

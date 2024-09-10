using System.Collections;
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
    void Start()
    {
        rend = GetComponent<Renderer>(); // renderuje nam zmienjacy sie kolor
        StartCoroutine(ReplenishNutrientsCoroutine());
    }

    IEnumerator ReplenishNutrientsCoroutine()
    {
        ReplenishNutrients();
        yield return new WaitForSeconds(replenishmentInterval);
    }
    public float ExtractNutrients(Plant plant, float amount)
    {
        float nutrientsGiven = Mathf.Min(amount, nutrients);
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

    public void ReplenishNutrients(float amount)
    {
        nutrients += amount;
        if (nutrients > maxNutrients)
        {
            nutrients = maxNutrients;
        }
    }
}
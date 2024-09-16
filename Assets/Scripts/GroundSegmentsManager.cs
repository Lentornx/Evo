using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSegmentsManager : MonoBehaviour
{
    private GroundSegment[] segments;
    public float replenishmentInterval = 4.0f;
    void Start()
    {
        segments = GetComponentsInChildren<GroundSegment>();
        InvokeRepeating("ReplenishNutrients", replenishmentInterval, replenishmentInterval);
    }

    private void ReplenishNutrients()
    {
        foreach (GroundSegment segment in segments)
        {
            segment.ReplenishNutrients();
        }
    }
}

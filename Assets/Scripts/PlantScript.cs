using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.EditorTools;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float range = 0.15f; // range of roots PER SIZE 
    private float finalRange;
    public float growthRate = 0.5f;  // How much the plant grows per unit of nutrient extracted
    public float baseNutrientConsumption = 1.0f; // max nutrient extracted from one tile
    public float adjustedNutrientConsumption;

    public float growthProgress = 0.0f;  // Tracks progress towards the next growth step
    private float growthThreshold = 100f; // how much growth needed to grow PER SIZE
    private float growthSurplusThreshold = 50f; // plant will only grow when threshold is exceeded by this value PER SIZE
    public float seedGrowth = 0;
    public float seedGrowthRatio = 0.1f; // jak¹ czêœæ seedGrowth dostaje z nutrientów
    public float maintanenceBase = 0.005f; // how much energy is spent PER SIZE per cycle
    public float seedCost = 0.1f;
    public float seedEfficiency = 0.2f; // ratio zu¿ytego progressu do tego co powstanie w nowym nasionku
    public float finalGrowthThreshold;
    // maintanence is to be changed into a function dependent on other parameters

    public int height = 1;
    public int heightCeiling = 20; // how many times can the plant grow?

    public int lifespan = 0; // rosniie co consume interval
    public int lifespanCeiling = 240;

    public float decayMultplier = 2.0f;
    public float metabolism = 1.0f;
    public float growthInterval;  // PER SIZE ile musi poczekac po poprzednim zwiekszeniu size
    public float consumeInterval; // idea - metabolism will affect how long this interval is?? a moze to jest nasz metabolism? moze niech on affectuje jak dlugo zyje roslina?
    public float seedInterval;

    public bool dynamicMat = false;
    private float initLocalScale = 0.1f;

    public bool isAlive = true;
    private int deathTimer; // uzywany kiedy dednie drzewo 
    public Material defaultMaterial;
    public Material deadMaterial;
    public Material struggleMaterial;

    private Renderer rend;

    public GameObject seed;
    private static PoolManager poolManager;
    private static Transform seedParent;

    private Dictionary<GroundSegment, SegmentInfo> AccessedSegments = new Dictionary<GroundSegment, SegmentInfo>(); //contains distance from this segment
    private static Collider[] hitColliders; //cached, STATIC WILL ONLY WORK IF GROW WILL REMAIN ON MAIN THREAD

    public struct SegmentInfo
    {
        public float squaredDistance;
        public float proximityFactor;

        public SegmentInfo(float squaredDistance, float proximityFactor)
        {
            this.squaredDistance = squaredDistance;
            this.proximityFactor = proximityFactor;
        }
    }
    // mo¿liwe ¿e maintanence i range podzielimy na base i increment
    void Start()
    {
        if (hitColliders == null)
            hitColliders = new Collider[50000]; // expected number
        if (poolManager == null)
            poolManager = FindObjectOfType<PoolManager>();
        if (seedParent == null)
            seedParent = GameObject.Find("Seeds").transform;
        if (rend == null)
            rend = GetComponent<Renderer>();
    }

    public void SetupVariables(float v1, float v2, int v3, float v4, float v5, float v6) // resetujemy tu te¿ pewne staty dla object poolingu
    {
        if (rend != null)
        {
            rend = GetComponent<Renderer>(); 
            rend.material = defaultMaterial;
        } 
        height = 1;
        finalRange = height * range;
        lifespan = 0;
        isAlive = true;
        transform.localScale = new Vector3(initLocalScale, initLocalScale, initLocalScale);
        CalculateFinalThreshold();

        
        baseNutrientConsumption = v1;
        growthSurplusThreshold = v2;
        heightCeiling = v3;
        metabolism = v4;
        seedCost = v5;
        growthProgress = v6;

        growthInterval = metabolism * 2.0f;
        consumeInterval = metabolism;
        seedInterval = metabolism * 60.0f;
        Invoke("StartMetabolism", 1.0f);
    }

    public void StartMetabolism() 
    {
        ExtractSegments(true);
        Invoke("ExtractNutrients", consumeInterval);
        Invoke("Grow", growthInterval);
        Invoke("SpawnSeed", seedInterval);
    }

    void ExtractSegments(bool add)
    {
        GroundSegment segment;
        Collider collider;
        int numColliders = Physics.OverlapSphereNonAlloc(new Vector3(transform.position.x, 0.0f, transform.position.z), finalRange, hitColliders);
        // Debug.Log("Number of colliders found: " + numColliders);
        for (int i = 0; i < numColliders; i++)
        {
            collider = hitColliders[i];
            segment = collider.GetComponent<GroundSegment>();
            if (segment != null)
            {
                if (add)
                {
                    StartExtracting(segment);
                }
                else
                {
                    if (!isAlive)
                        AccessedSegments.Remove(segment);
                    else
                        segment.RemoveFromExtraction(this);
                }
            }

        }
    }


    public void StartExtracting(GroundSegment segment) // raczej wrócimy do dok³adniejszych dystansów z Vector3.distance
    {
        float proximityFactor;
        SegmentInfo segmentInfo;
        float squaredRange = (finalRange) * (finalRange); //to zawsze ustawiamy na nowo bo ta funkcja odpala siê tylko przy zmianie finalRange
        bool isSegmentPresent = AccessedSegments.ContainsKey(segment);

        if (!isSegmentPresent) // distance has to be calculated
        {
            Vector3 SegmentPositionXZ = new Vector3(segment.transform.position.x, 0, segment.transform.position.z);
            float squaredDistance = (transform.position - SegmentPositionXZ).sqrMagnitude;

            proximityFactor = Mathf.Clamp01(1.0f - (squaredDistance / squaredRange));
            SegmentInfo newInfo = new SegmentInfo(squaredDistance, proximityFactor);
            segmentInfo = new SegmentInfo(squaredDistance, proximityFactor);
            AccessedSegments.Add(segment, segmentInfo);
            segment.addToExtraction(this, adjustedNutrientConsumption / consumeInterval);
        }
        else // distance already cached
        {
            proximityFactor = Mathf.Clamp01(1.0f - (AccessedSegments[segment].squaredDistance / squaredRange));
            segmentInfo = new SegmentInfo(AccessedSegments[segment].squaredDistance, proximityFactor);
            AccessedSegments[segment] = segmentInfo;
        }
    }

    void ExtractNutrients()
    {
        if (isAlive)
        {
            Invoke("ExtractNutrients", consumeInterval);
            float totalNutrientConsumed = 0;
            CalculateConsumption();

            foreach (var segment in AccessedSegments)
            {
                if (segment.Key.HasNutrients() && segment.Value.proximityFactor > 0)
                {
                    totalNutrientConsumed += segment.Key.ExtractNutrients(this, adjustedNutrientConsumption * segment.Value.proximityFactor, consumeInterval);
                }
            }

            float growthNetGain = totalNutrientConsumed * growthRate;
            growthProgress += growthNetGain * (1.0f - seedGrowthRatio);
            seedGrowth += growthNetGain * seedGrowthRatio;

            lifespan++;
            float maintanenceTotal = maintanenceBase * height;

            growthProgress -= maintanenceTotal; // odejmujemy utrzymanie organizmu od progressu

            float progressRatio =  growthProgress / ((growthThreshold + growthSurplusThreshold) * height); 
            if(dynamicMat)
                rend.material.color = Color.Lerp(struggleMaterial.color, defaultMaterial.color, progressRatio); 
                // pokazuje jak du¿y ma zapas growth wzglêdem nastêpnego progu wzrostu

            if (growthProgress < 0 || lifespan > lifespanCeiling) // deda jak nic mu nie zostaje
            {
                turnDead();
            }
        }
    }

    private void CalculateConsumption()  // funkcja to spowolnienia pobierania nutrientów gdy roœlina jest przepe³niona
    {
        adjustedNutrientConsumption = baseNutrientConsumption * Mathf.Clamp(Mathf.Pow(((growthThreshold + growthSurplusThreshold) * height) / growthProgress, 3), 0.001f, 1.0f);
    }

    void Grow()
    {
        if (isAlive)
        {
            if ((growthProgress >= (growthThreshold + growthSurplusThreshold) * height) && height < heightCeiling)
            // jezeli progress wzrostu jest na odpowiednim poziomie oraz nie osiognieta maksymalnej wielkosci
            {
                transform.localScale += new Vector3(0.01f, 0.1f, 0.01f);
                transform.position += new Vector3(0.0f, 0.1f, 0.0f);
                // wydluzamy i przesuwamy do gory (czysto estetyczny efekt)
                growthProgress -= growthThreshold * height; // zmiejszamy progress o ilosc wymagana do wzrostu
                height++;
                finalRange = height * range;
                CalculateFinalThreshold();
                ExtractSegments(true); // dodajemy nowe segmenty, bo wraz z size zwieksza sie m.in. zasieg
                Invoke("Grow", growthInterval * height);
            }
            else //jezeli jest max size to moze juz wgl bez invoke?
                Invoke("Grow", growthInterval);
        }
    }

    void CalculateFinalThreshold() 
    {
        finalGrowthThreshold = (growthSurplusThreshold + growthThreshold) * height;
    }

    void SpawnSeed() //powinno zabierac troche progressu, okolo tyle ile seed dostaje na poczatek zycia rosliny
    {
        if (isAlive)
        {
            float newBaseNutrientConsumption = baseNutrientConsumption;
            float newGrowthSurplusThreshold = growthSurplusThreshold;
            int newHeightCeiling = heightCeiling;
            float newMetabolism = metabolism;
            float newSeedCost = seedCost;
            float mutateRate = 1f; // mo¿e do mutowania

            int seedCount = (int)(seedGrowth / seedCost);
            seedGrowth -= seedCost * seedCount;
            Debug.Log(growthProgress);
            int seedCountFromGrowthOverflow = (int)((growthProgress - finalGrowthThreshold) / seedCost);
            growthProgress -= seedCountFromGrowthOverflow * seedCost;
            Debug.Log(seedCountFromGrowthOverflow * seedCost);
            Debug.Log(growthProgress);
            seedCountFromGrowthOverflow = Mathf.RoundToInt(seedCountFromGrowthOverflow * seedEfficiency);
            seedCount += seedCountFromGrowthOverflow;

            for (int i = 0; i < seedCount; i++)
            {
                float[] mutate = new float[] { Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f) };
                // zapis tablicy do skrócenia bo umrzemy albo bêdziemy po prostu parametryCount razy losowali z range
                if (mutate[0] < mutateRate)
                {
                    newBaseNutrientConsumption = baseNutrientConsumption * Random.Range(0.9f, 1.1f);
                }
                if (mutate[1] < mutateRate)
                {
                    newGrowthSurplusThreshold = growthSurplusThreshold * Random.Range(0.9f, 1.1f);
                }
                if (mutate[2] < mutateRate)
                {
                    newHeightCeiling = Mathf.RoundToInt(heightCeiling * Random.Range(0.9f, 1.1f));
                }
                if (mutate[3] < mutateRate)
                {
                    newMetabolism = metabolism * Random.Range(0.9f, 1.1f);
                }
                if (mutate[4] < mutateRate)
                {
                    newSeedCost = seedCost * Random.Range(0.9f, 1.1f);
                }
                GameObject newObj = poolManager.GetSeed(new Vector3(transform.position.x, transform.position.y + Random.Range(0.0f, transform.position.y), transform.position.z), Quaternion.identity);
                // kiedy bêd¹ branche to bêdziemy spawnowaæ z ró¿nych liœci
                Seed newSeed = newObj.GetComponent<Seed>();
                newSeed.SetupVariables(newBaseNutrientConsumption, newGrowthSurplusThreshold, newHeightCeiling, newMetabolism, newSeedCost, seedCost * seedEfficiency);
            }
            // stworzyæ funkcjê mutateAndSpawn i pozbyæ siê ca³ego tego fora st¹d 

            Invoke("SpawnSeed", seedInterval);
        }
    }

    void turnDead() // zamienia siê w martw¹ roœlinê tera :OO
    {
        deathTimer = height;
        ExtractSegments(false);
        isAlive = false;
        Invoke("Decay", growthInterval * (height));
        // zmieniamy material dla czytelnosci
        if (rend != null)
        {
            rend.material = deadMaterial;
        }
    }

    void Decay() 
    {
        deathTimer--;
        if (deathTimer > 0)
        {
            Invoke("Decay", growthInterval * deathTimer);
            foreach (var segment in AccessedSegments)
            {
                float amount = deathTimer * segment.Value.proximityFactor * decayMultplier; 
                segment.Key.ReplenishNutrients(amount);
            }
            transform.position -= new Vector3(0.00f, 0.1f, 0.00f);
            transform.localScale -= new Vector3(0.00f, 0.1f, 0.00f);
        }
        else
        {
            ExtractSegments(false);
            poolManager.ReturnPlant(gameObject);
        }
    }

    void OnDrawGizmosSelected() // pokazuje zasieg collidera od korzeni
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0.0f, transform.position.z), finalRange);
    }
}

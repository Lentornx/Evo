using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float range = 0.15f; // range of roots PER SIZE 
    public float growthRate = 0.001f;  // How much the plant grows per unit of nutrient extracted
    public float baseNutrientConsumption = 1.0f; // max nutrient extracted from one tile
    public float adjustedNutrientConsumption;

    public float growthProgress = 0.0f;  // Tracks progress towards the next growth step
    public float growthThreshold = 0.1f; // how much growth needed to grow PER SIZE
    public float growthSurplusThreshold = 0.05f; // plant will only grow when threshold is exceeded by this value PER SIZE
    public float seedGrowth = 0;
    public float seedGrowthRatio = 0.1f; // jak¹ czêœæ seedGrowth dostaje z nutrientów
    public float maintanenceBase = 0.005f; // how much energy is spent PER SIZE per cycle
    public float seedCost = 0.1f;
    // maintanence is to be changed into a function dependent on other parameters

    public int height = 1;
    public int heightCeiling = 20; // how many times can the plant grow?

    public int lifespan = 0; // rosniie co consume interval
    public int lifespanCeiling = 240;

    public float metabolism = 1.0f;
    public float growthIntervalBase;  // PER SIZE ile musi poczekac po poprzednim zwiekszeniu size
    public float consumeInterval; // idea - metabolism will affect how long this interval is?? a moze to jest nasz metabolism? moze niech on affectuje jak dlugo zyje roslina?
    public float seedInterval;

    public bool dynamicMat = false;

    public bool isAlive = true;
    public Material defaultMaterial;
    public Material deadMaterial;
    public Material struggleMaterial;

    private Renderer rend;

    public GameObject seed;
    private Transform seedParent;
    private HashSet<GroundSegment> AccessedSegments = new HashSet<GroundSegment>();

    // mo¿liwe ¿e maintanence i range podzielimy na base i increment

    void Start()
    {
        seedParent = GameObject.Find("Seeds").transform; // zapisuje miejsce w hierarchi obiektow gdzie bedzie dodawal seedy
        rend = GetComponent<Renderer>();
    }
    public void SetupVariables(float v1, float v2, int v3, float v4, float v5, float v6)
    {
        ExtractSegments(true);

        baseNutrientConsumption = v1;
        growthSurplusThreshold = v2;
        heightCeiling = v3;
        metabolism = v4;
        seedCost = v5;
        growthProgress = v6;

        growthIntervalBase = metabolism * 2.0f;
        consumeInterval = metabolism;
        seedInterval = metabolism * 30.0f;

        Invoke("ExtractNutrients", consumeInterval);
        Invoke("Grow", growthIntervalBase);
        Invoke("SpawnSeed", seedInterval);
    }

    void ExtractSegments(bool add) // parametr mowiacy czy zaczynam extrakcje nowych segmentow czy je odlaczam gdy roslina deda
    {
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(transform.position.x, 0.0f, transform.position.z), range * height);
        // kulisty kollider u bazy roœliny do symulowania zasiêgu korzeni
        foreach (var hitCollider in hitColliders)
        {
            GroundSegment segment = hitCollider.GetComponent<GroundSegment>();
            if (segment != null)
            {
                if (add)
                {
                    if (isAlive)
                        segment.StartExtracting(this);
                    AccessedSegments.Add(segment);
                }
                else //remove
                {
                    if (isAlive)
                        segment.StopExtracting(this);
                    AccessedSegments.Remove(segment);
                }
            }
        }
    }

    void ExtractNutrients()
    {
        if (isAlive)
        {
            Invoke("ExtractNutrients", consumeInterval);

            float totalNutrientConsumed = 0;
            adjustedNutrientConsumption = baseNutrientConsumption * Mathf.Clamp(Mathf.Pow(((growthThreshold + growthSurplusThreshold) * height) / growthProgress, 3), 0.01f, 1.0f);
            // funkcja to spowolnienia pobierania nutrientów gdy roœlina jest przepe³niona
            foreach (var segment in AccessedSegments)
            {
                if (segment != null && segment.HasNutrients())
                {
                    totalNutrientConsumed += segment.ExtractNutrients(this, adjustedNutrientConsumption);
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
                ExtractSegments(true); // dodajemy nowe segmenty, bo wraz z size zwieksza sie m.in. zasieg
                Invoke("Grow", growthIntervalBase * height);
            }
            else //jezeli jest max size to moze juz wgl bez invoke?
                Invoke("Grow", growthIntervalBase);
        }
    }

    void SpawnSeed() //powinno zabierac troche progressu, okolo tyle ile seed dostaje na poczatek zycia rosliny
    {

        if (isAlive)
        {
            if (lifespan > lifespanCeiling / 10)
            {
                float newBaseNutrientConsumption = baseNutrientConsumption;
                float newGrowthSurplusThreshold = growthSurplusThreshold;
                int newHeightCeiling = heightCeiling;
                float newMetabolism = metabolism;
                float newSeedCost = seedCost;
                float mutateRate = 1f; // mo¿e do mutowania

                int seedCount = (int)(seedGrowth / seedCost);
                seedGrowth -= seedCost * seedCount;
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
                    GameObject newObj = Instantiate(seed, new Vector3(transform.position.x, transform.position.y + Random.Range(0.0f, transform.position.y), transform.position.z), Quaternion.identity, seedParent);
                    // kiedy bêd¹ branche to bêdziemy spawnowaæ ró¿nych liœci
                    Seed newSeed = newObj.GetComponent<Seed>();
                    newSeed.SetupVariables(newBaseNutrientConsumption, newGrowthSurplusThreshold, newHeightCeiling, newMetabolism, newSeedCost, seedCost);
                }
                // stworzyæ funkcjê mutateAndSpawn i pozbyæ siê ca³ego tego fora st¹d 
            }
            Invoke("SpawnSeed", seedInterval);
        }
    }

    void turnDead() // zamienia siê w martw¹ roœlinê tera :OO
    {
        ExtractSegments(false); // odlaczamy segmenty od rosliny bo dednie i nie bêdzie ju¿ pobiera³a niczego
        isAlive = false;
        Invoke("Decay", growthIntervalBase * (height / 2.0f));

        Renderer renderer = GetComponent<Renderer>(); // zmieniamy material dla czytelnosci
        if (renderer != null)
        {
            renderer.material = deadMaterial;
        }
    }

    void Decay()
    {
        if (height > 0)
        {
            Invoke("Decay", growthIntervalBase * (height / 2.0f));
            ExtractSegments(true);
            foreach (var segment in AccessedSegments)
            {
                if (segment != null)
                {
                    segment.GetNutrientsFromDecay(this);
                }
            }
            ExtractSegments(false);
            height--;
            transform.position -= new Vector3(0.01f, 0.1f, 0.01f);
            transform.localScale -= new Vector3(0.0f, 0.1f, 0.0f);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    void OnDrawGizmosSelected() // pokazuje zasieg collidera od korzeni
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0.0f, transform.position.z), range * height);
    }
}

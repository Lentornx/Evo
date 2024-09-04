using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Plant : MonoBehaviour
{
    public float range = 0.15f; // range of roots PER SIZE 
    public float growthRate = 0.001f;  // How much the plant grows per unit of nutrient extracted
    public float baseNutrientConsumption = 1.0f; // max nutrient extracted from one tile
    public float adjustedNutrientConsumption;

    public float growthProgress = 0.0f;  // Tracks progress towards the next growth step
    public float growthThreshold = 0.1f; // how much growth needed to grow PER SIZE
    public float growthSurplusThreshold = 0.05f; // plant will only grow when threshold is exceeded by this value PER SIZE
    public float maintanence = 0.05f; // how much energy is spent PER SIZE per cycle

    public int size = 1;
    public int sizeCeiling = 20; // how many times can the plant grow?

    public int lifespan = 0; // rosniie co consume interval
    public int lifespanCeiling = 240;

    public float growthIntervalBase = 2.0f;  // PER SIZE ile musi poczekac po poprzednim zwiekszeniu size
    public float consumeInterval = 1.0f; // idea - metabolism will affect how long this interval is?? a moze to jest nasz metabolism? moze niech on affectuje jak dlugo zyje roslina?
    public float seedInterval = 10.0f;

    public bool isAlive = true;
    public Material deadMaterial;

    public GameObject Seed;
    private Transform SeedParent;
    private HashSet<GroundSegment> AccessedSegments = new HashSet<GroundSegment>(); 

    // mo¿liwe ¿e maintanence i range podzielimy na base i increment

    void Start()
    {
        SeedParent = GameObject.Find("Seeds").transform; // zapisuje miejsce w hierarchi obiektow gdzie bedzie dodawal seedy
        ExtractSegments(true);
        Invoke("ExtractNutrients", consumeInterval);
        Invoke("Grow", growthIntervalBase);
        Invoke("SpawnSeed", seedInterval); 
    }

    void ExtractSegments(bool add) // parametr mowiacy czy zaczynam extrakcje nowych segmentow czy je odlaczam gdy roslina deda
    {
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(transform.position.x, 0.0f, transform.position.z), range * size); 
        // kulisty kollider u bazy roœliny do symulowania zasiêgu korzeni
        foreach (var hitCollider in hitColliders)
        {
            GroundSegment segment = hitCollider.GetComponent<GroundSegment>();
            if (segment != null)
            {
                if (add)
                {
                    if(isAlive)
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
            adjustedNutrientConsumption = baseNutrientConsumption * Mathf.Clamp(Mathf.Pow(((growthThreshold + growthSurplusThreshold) * size) / growthProgress, 3), 0.01f, 1.0f);
            // funkcja to spowolnienia pobierania nutrientów gdy roœlina jest przepe³niona
            foreach (var segment in AccessedSegments)
            {
                if (segment != null && segment.HasNutrients())
                {
                    totalNutrientConsumed += segment.ExtractNutrients(this, adjustedNutrientConsumption);
                }
            }
            growthProgress += totalNutrientConsumed * growthRate;

            lifespan++;
            growthProgress -= maintanence * size; // odejmujemy utrzymanie organizmu od progressu
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
            if ((growthProgress >= (growthThreshold + growthSurplusThreshold) * size) && size < sizeCeiling)
            // jezeli progress wzrostu jest na odpowiednim poziomie oraz nie osiognieta maksymalnej wielkosci
            {
                transform.localScale += new Vector3(0.0f, 0.1f, 0.0f);
                transform.position += new Vector3(0.0f, 0.05f, 0.0f);
                // wydluzamy i przesuwamy do gory (czysto estetyczny efekt)
                growthProgress -= growthThreshold * size; // zmiejszamy progress o ilosc wymagana do wzrostu
                size++;
                ExtractSegments(true); // dodajemy nowe segmenty, bo wraz z size zwieksza sie m.in. zasieg
                Invoke("Grow", growthIntervalBase * size);
            }
            else //jezeli jest max size to moze juz wgl bez invoke?
                Invoke("Grow", growthIntervalBase);
        }
    }

    void SpawnSeed() //powinno zabierac troche progressu, okolo tyle ile seed dostaje na poczatek zycia rosliny
    {
        if (isAlive)
        {
            if (size > sizeCeiling / 2)
            {
                Instantiate(Seed, transform.position, Quaternion.identity, SeedParent);
            }
            Invoke("SpawnSeed", seedInterval);
        }
    }

    void turnDead() // zamienia siê w martw¹ roœlinê tera :OO
    {
        ExtractSegments(false); // odlaczamy segmenty od rosliny bo dednie i nie bêdzie ju¿ pobiera³a niczego
        isAlive = false;
        Invoke("Decay", growthIntervalBase * (size / 2.0f));

        Renderer renderer = GetComponent<Renderer>(); // zmieniamy material dla czytelnosci
        if (renderer != null)
        {
            renderer.material = deadMaterial;  
        }
    }

    void Decay()
    {
        if(size > 0)
        {
            Invoke("Decay", growthIntervalBase * (size/2.0f));
            ExtractSegments(true);
            foreach (var segment in AccessedSegments)
            {
                if (segment != null)
                {
                    segment.GetNutrientsFromDecay(this);
                }
            }
            ExtractSegments(false);
            size--;
            transform.position -= new Vector3(0.0f, 0.05f, 0.0f);
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
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0.0f, transform.position.z), range * size);
    }
}

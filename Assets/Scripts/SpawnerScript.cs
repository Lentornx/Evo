using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject groundSegment;
    public GameObject Plant;
    public GameObject Seed;
    public int rows = 50;
    public int columns = 50;
    public float segmentSize = 0.1f;

    private GameObject groundParent;
    private GameObject plantParent;
    private GameObject SeedParent;

    void Start()
    {
        // Create parent objects to organize the hierarchy
        groundParent = new GameObject("GroundSegments");
        groundParent.transform.parent = transform;
        SeedParent = new GameObject("Seeds");
        SeedParent.transform.parent = transform;
        plantParent = new GameObject("Plants");
        plantParent.transform.parent = transform;

        GenerateGround();
        GenerateSeed();
    }

    void GenerateGround()
        // w przyszlosci bedziemy tez spawnowali kawalki pod ziemia, symulujac glebsze polacie gleby
    {
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 position = new Vector3(x * segmentSize, 0, z * segmentSize);
                Instantiate(groundSegment, position, Quaternion.identity, groundParent.transform);
            }
        }
    }

    // kilka wariantow do testow, pozniej bedziemy generowac proceduralnie
    void GeneratePlants()
    {
        Vector3 position = new Vector3(2f, 0.1f, 2f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);

        position = new Vector3(4f, 0.1f, 4f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);

        position = new Vector3(4f, 0.1f, 2f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);

        position = new Vector3(2f, 0.1f, 4f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);

        position = new Vector3(3f, 0.1f, 3f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);
    }

    void Generate2Plants()
    {
        Vector3 position = new Vector3(0.0f, 0.1f, 0.0f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);
        position = new Vector3(0.3f, 0.1f, 0.0f);
        Instantiate(Plant, position, Quaternion.identity, plantParent.transform);
    }
    void GenerateSeed()
    {
        Vector3 position = new Vector3(3f, 0.4f, 3f);
        Instantiate(Seed, position, Quaternion.identity, SeedParent.transform);
    }
}

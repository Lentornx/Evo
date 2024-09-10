using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject groundSegment;
    public GameObject Plant;
    public GameObject Seed;
    public GameObject decoy;
    public int rows = 50;
    public int columns = 50;
    public float segmentSize = 0.1f;
    public float decoySize = 0.1f;

    private GameObject groundParent;
    public GameObject plantParent;
    public GameObject SeedParent;

    void Start()
    {
        // Create parent objects to organize the hierarchy
        groundParent = new GameObject("GroundSegments");
        groundParent.transform.parent = transform;

        GenerateGround();
        GenerateDecoy();
        GenerateSeed();
        //CombineGroundMeshes(); // Combine ground meshes into one
    }

    void GenerateGround()
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

    void GenerateDecoy()
    {
        // Calculate the size and position of the decoy
        float width = columns * segmentSize;
        float height = rows * segmentSize;
        Vector3 decoyPosition = new Vector3(width / 2, 0, height / 2);

        // Instantiate the decoy
        GameObject largeDecoy = Instantiate(decoy, decoyPosition, Quaternion.identity);

        // Scale the decoy to cover the grid
        largeDecoy.transform.localScale = new Vector3(width * decoySize, 1, height * decoySize);
    }

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
        Vector3 position = new Vector3(rows*segmentSize/2, 0.4f, rows*segmentSize/2);
        Instantiate(Seed, position, Quaternion.identity, SeedParent.transform);
    }

    void CombineGroundMeshes()
    {
        MeshFilter[] meshFilters = groundParent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        MeshFilter filter = groundParent.AddComponent<MeshFilter>();
        MeshRenderer renderer = groundParent.AddComponent<MeshRenderer>();

        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);

        // Optionally, assign the same material to the combined mesh
        renderer.material = meshFilters[0].GetComponent<Renderer>().sharedMaterial;

        // Optionally, remove the original ground segment objects
        foreach (var mf in meshFilters)
        {
            Destroy(mf.gameObject);
        }
    }
}

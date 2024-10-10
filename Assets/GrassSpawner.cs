using UnityEngine;

public class GrassSpawner : MonoBehaviour
{

    public GameObject grass;
    public float range = 15f;
    public float density = 1f;
    // Start is called before the first frame update
    void Start()
    {
        float area = Mathf.PI * Mathf.Pow(range, 2);
        int numberOfGrassInstances = Mathf.FloorToInt(density * area);

        for (int i = 0; i < numberOfGrassInstances; i++)
        {
            // Generate a random position within the defined range
            float randomX = Random.Range(-range, range);
            float randomZ = Random.Range(-range, range);

            // Calculate the position in the x-z plane
            Vector3 randomPosition = new Vector3(randomX, Random.Range(-0.8f, 0.2f), randomZ) + transform.position;

            float randomY = Random.Range(0f, 360f); // Random angle between 0 and 360 degrees

            // Create the quaternion with -90 on the x-axis, random on the y-axis, and 0 on the z-axis
            Quaternion randomRotation = Quaternion.Euler(-90f, randomY, Random.Range(-25f, 25f));
            
            // Instantiate the grass prefab at the random position
            Instantiate(grass, randomPosition, randomRotation);
        }
    }
}

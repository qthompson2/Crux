using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Placement Settings")]
    public Terrain targetTerrain;
    public GameObject[] treePrefabs;
    public int treeCount = 500;
    public string containerName = "Spawned Trees";

    [Header("Placement Options")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public bool alignToNormal = true;

    public void SpawnTrees()
    {
        if (targetTerrain == null)
        {
            Debug.LogError("No terrain assigned!");
            return;
        }

        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogError("No tree prefabs assigned!");
            return;
        }

        TerrainData terrainData = targetTerrain.terrainData;
        Vector3 terrainPos = targetTerrain.transform.position;

        // Create a parent container
        GameObject container = new GameObject(containerName);
        container.transform.position = terrainPos;

        for (int i = 0; i < treeCount; i++)
        {
            // Pick a random tree prefab
            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Random position on the terrain
            float randX = Random.Range(0f, terrainData.size.x);
            float randZ = Random.Range(0f, terrainData.size.z);
            float height = terrainData.GetInterpolatedHeight(randX / terrainData.size.x, randZ / terrainData.size.z);

            Vector3 worldPos = new Vector3(randX + terrainPos.x, height + terrainPos.y, randZ + terrainPos.z);

            // Align rotation to terrain normal
            Quaternion rotation = Quaternion.identity;
            if (alignToNormal)
            {
                Vector3 normal = terrainData.GetInterpolatedNormal(randX / terrainData.size.x, randZ / terrainData.size.z);
                rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }

            GameObject tree = Instantiate(prefab, worldPos, rotation, container.transform);

            // Random uniform scale
            float randomScale = Random.Range(minScale, maxScale);
            tree.transform.localScale = Vector3.one * randomScale;
        }

        Debug.Log($"Spawned {treeCount} trees into '{container.name}'.");
    }

    // Editor button for quick use
    [ContextMenu("Spawn Trees Now")]
    private void SpawnTreesButton()
    {
        SpawnTrees();
    }
}

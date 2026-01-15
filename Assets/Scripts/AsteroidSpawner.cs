using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AsteroidSpawner : NetworkBehaviour
{
    public static AsteroidSpawner Instance { get; private set; }

    public int number;
    public int depth;
    public NetworkPrefabRef asteroidPrefab;

    private List<NetworkObject> rootAsteroids;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        rootAsteroids = new List<NetworkObject>();
    }

    public void SpawnAsteroids(NetworkRunner runner)
    {
        if (!runner.IsServer)
            return;

        Camera camera = Camera.main;

        for (int i = 0; i < number; i++)
        {
            Vector3 viewportPosition = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 10f);
            Vector3 worldPosition = camera.ViewportToWorldPoint(viewportPosition);
            worldPosition.z = 0.0f;

            NetworkObject rootAsteroid = CreateAsteroidTree(runner, worldPosition);

            rootAsteroids.Add(rootAsteroid);
        }
    }

    public void DespawnAsteroids()
    {
        if (Object == null || !Object.HasStateAuthority) return;

        if (rootAsteroids == null || rootAsteroids.Count == 0)
        {
            Debug.Log("There are no root asteroids to despawn!");
            return;
        }

        foreach (var asteroid in rootAsteroids)
        {
            if (asteroid != null && asteroid.IsValid)
            {
                Runner.Despawn(asteroid);
            }
        }

        rootAsteroids.Clear();
    }

    NetworkObject CreateAsteroidTree(NetworkRunner runner, Vector3 worldPos)
    {
        NetworkObject root = runner.Spawn(asteroidPrefab, worldPos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        var treeNode = root.GetComponent<TreeNode>();
        if (treeNode != null && depth > 0)
        {
            treeNode.SpawnChildren(depth, root, asteroidPrefab);
        }

        return root;
    }
}
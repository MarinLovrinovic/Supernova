using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : NetworkBehaviour
{
    public static AsteroidSpawner Instance { get; private set; }

    public int number;
    public int depth;

    [SerializeField] private NetworkObject asteroidPrefab;

    private List<NetworkObject> rootAsteroids = new();

    public override void Spawned()
    {
        if(Object.HasStateAuthority)
            SpawnAsteroids();
    }
    private void Awake()
    {
        Instance = this;
    }

    public void SpawnAsteroids()
    {
        if (!Object.HasStateAuthority)
            return;

        Camera camera = Camera.main;

        for (int i = 0; i < number; i++)
        {
            Vector3 viewportPosition = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), camera.nearClipPlane + 10f);
            Vector3 worldPosition = camera.ViewportToWorldPoint(viewportPosition);
            worldPosition.z = 0.0f;

            NetworkObject rootAsteroid = Runner.Spawn(asteroidPrefab, worldPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            rootAsteroids.Add(rootAsteroid);

            TreeNode node = rootAsteroid.GetComponent<TreeNode>();
            if (node != null)
                node.SpawnChildrenRpc(depth);
        }
    }

    public void DespawnAsteroids()
    {
        if (!Object.HasStateAuthority)
            return;

        foreach (var asteroid in rootAsteroids)
        {
            if (asteroid != null)
                Runner.Despawn(asteroid);
        }

        rootAsteroids.Clear();
    }
}

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
    private GameObject asteroidContainer;

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
        asteroidContainer = new GameObject("AsteroidContainer");
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnAsteroids(NetworkRunner runner)
    {
        if (!runner.IsServer)
            return;
        
        Camera camera = Camera.main;

        for (int i = 0; i < number; i++)
        {
            Vector3 viewportPosition = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), camera.nearClipPlane + 10f);
            Vector3 worldPosition = camera.ViewportToWorldPoint(viewportPosition);
            worldPosition.z = 0.0f;

            NetworkObject rootAsteroid = CreateAsteroidTree(runner, worldPosition);
            rootAsteroid.transform.SetParent(asteroidContainer.transform, worldPositionStays: true);
            rootAsteroids.Add(rootAsteroid);
        }
    }

    public void DespawnAsteroids()
    {
        if (rootAsteroids.Count == 0)
        {
            Debug.Log("There are no root asteroids!");
            return;
        }

        for (int i = 0; i < rootAsteroids.Count; i++)
        {
            Destroy(rootAsteroids[i]);
        }

        rootAsteroids.Clear();
    }

    NetworkObject CreateAsteroidTree(NetworkRunner runner, Vector3 worldPos)
    {
        NetworkObject root = runner.Spawn(asteroidPrefab, worldPos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (root.GetComponent<TreeNode>() == null) return root;
        if (depth <= 0) return root;

        root.GetComponent<TreeNode>().SpawnChildren(depth, root, asteroidPrefab);

        return root;
    }
}

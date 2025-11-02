using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public int number;
    public int depth;
    [SerializeField] private GameObject asteroidPrefab;

    private GameObject[] asteroids;
    private GameObject asteroidContainer;

    void Start()
    {
        asteroids = new GameObject[number];
        asteroidContainer = new GameObject("AsteroidContainer");

        Camera camera = Camera.main;

        for (int i = 0; i < number; i++)
        {
            Vector3 viewportPosition = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), camera.nearClipPlane + 10f);
            Vector3 worldPosition = camera.ViewportToWorldPoint(viewportPosition);
            worldPosition.z = 0.0f;

            GameObject asteroid = CreateAsteroidTree(worldPosition);
            asteroid.transform.SetParent(asteroidContainer.transform, worldPositionStays: true);
            asteroids[i] = asteroid;
        }
    }

    void Update()
    {
        
    }

    GameObject CreateAsteroidTree(Vector3 worldPos)
    {
        GameObject root = Instantiate(asteroidPrefab, worldPos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (root.GetComponent<TreeNode>() == null) return root;
        if (depth <= 0) return root;

        root.GetComponent<TreeNode>().SpawnChildren(depth, root, asteroidPrefab);

        return root;
    }
}

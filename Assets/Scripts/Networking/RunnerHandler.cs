using Fusion;
using UnityEngine;



public class RunnerHandler : MonoBehaviour
{
    void Awake()
    {
        var spawner = FindObjectOfType<SpaceshipSpawner>();
        var runner = GetComponent<NetworkRunner>();

        runner.AddCallbacks(spawner);
    }
}

using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class TreeNode : NetworkBehaviour
{
    public float relativeDepthScale = 0.5f;
    public float relativeOffset = 1.0f;
    public bool keepRelativePos = true;

    private TreeNode leftChild;
    private TreeNode rightChild;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    NetworkObject SpawnChild(out TreeNode child, NetworkObject parentObj, NetworkPrefabRef parentPrefab)
    {
        Vector3 offset = new Vector3(Random.Range(-relativeOffset, relativeOffset), Random.Range(-relativeOffset, relativeOffset), 0.0f);
        NetworkObject childObj = Runner.Spawn(
            parentPrefab,
            parentObj.transform.position + offset,
            Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)),
            PlayerRef.None,
            (runner, o) =>
            {
                o.transform.localScale = parentObj.transform.localScale * relativeDepthScale;
                o.transform.SetParent(parentObj.transform, worldPositionStays: keepRelativePos);
            });
        child = childObj.GetComponent<TreeNode>();

        return childObj;
    }

    public void SpawnChildren(int depth, NetworkObject parentObj, NetworkPrefabRef parentPrefab)
    {
        NetworkObject leftObj = SpawnChild(out leftChild, parentObj, parentPrefab);
        NetworkObject rightObj = SpawnChild(out rightChild, parentObj, parentPrefab);

        if (AsteroidSpawner.Instance != null) 
        {
            AsteroidSpawner.Instance.RegisterAsteroid(leftObj);  
            AsteroidSpawner.Instance.RegisterAsteroid(rightObj); 
        }
        
        depth--;

        if (depth > 0)
        {
            leftObj.GetComponent<TreeNode>().SpawnChildren(depth, leftObj, parentPrefab);
            rightObj.GetComponent<TreeNode>().SpawnChildren(depth, rightObj, parentPrefab);
        }

    }
}

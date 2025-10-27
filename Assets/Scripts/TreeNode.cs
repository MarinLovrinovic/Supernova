using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TreeNode : MonoBehaviour
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

    GameObject SpawnChild(TreeNode child, GameObject parentObj, GameObject parentPrefab)
    {
        Vector3 offset = new Vector3(Random.Range(-relativeOffset, relativeOffset), Random.Range(-relativeOffset, relativeOffset), 0.0f);
        GameObject childObj = Instantiate(parentPrefab, parentObj.transform.position + offset, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
        childObj.transform.localScale = parentObj.transform.localScale * relativeDepthScale;
        childObj.transform.SetParent(parentObj.transform, worldPositionStays: keepRelativePos);

        child = childObj.GetComponent<TreeNode>();

        return childObj;
    }

    public void SpawnChildren(int depth, GameObject parentObj, GameObject parentPrefab)
    {
        GameObject leftObj = SpawnChild(leftChild, parentObj, parentPrefab);
        GameObject rightObj = SpawnChild(rightChild, parentObj, parentPrefab);

        depth--;

        if (depth > 0)
        {
            leftObj.GetComponent<TreeNode>().SpawnChildren(depth, leftObj, parentPrefab);
            rightObj.GetComponent<TreeNode>().SpawnChildren(depth, rightObj, parentPrefab);
        }

    }
}

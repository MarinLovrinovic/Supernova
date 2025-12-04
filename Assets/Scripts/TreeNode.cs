using Fusion;
using UnityEngine;

public class TreeNode : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _treePrefab;

    public float relativeDepthScale = 0.5f;
    public float relativeOffset = 1.0f;
    public bool keepRelativePos = true;

    private TreeNode leftChild;
    private TreeNode rightChild;

    private TreeNode SpawnChild()
    {
        Vector3 offset = new Vector3(
            Random.Range(-relativeOffset, relativeOffset),
            Random.Range(-relativeOffset, relativeOffset),
            0.0f);

        NetworkObject obj = Runner.Spawn(
            _treePrefab,
            transform.position + offset,
            Quaternion.Euler(0, 0, Random.Range(0, 360)));

        obj.transform.localScale = transform.localScale * relativeDepthScale;
        obj.transform.SetParent(transform, worldPositionStays: keepRelativePos);

        return obj.GetComponent<TreeNode>();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void SpawnChildrenRpc(int depth)
    {
        SpawnChildren(depth);
    }

    public void SpawnChildren(int depth)
    {
        if (depth <= 0)
            return;

        leftChild = SpawnChild();
        rightChild = SpawnChild();

        depth--;

        if (depth > 0)
        {
            leftChild.SpawnChildren(depth);
            rightChild.SpawnChildren(depth);
        }
    }
}

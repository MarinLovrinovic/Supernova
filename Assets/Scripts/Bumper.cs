using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
public class Bumper : NetworkBehaviour
{
    public float bumpFactor = 5f;

    private Rigidbody2D thisBody;

    void Awake()
    {
        thisBody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority)
            return;

        if (!other.CompareTag("Player")) 
            return;

        if(other.GetComponent<NetworkRigidbody2D>() == null)
                return;

        Rigidbody2D rb = other.GetComponent<NetworkRigidbody2D>().Rigidbody;

        Vector2 direction = (other.transform.position - transform.position).normalized;

        rb.AddForce(direction * bumpFactor, ForceMode2D.Impulse);
    }
}

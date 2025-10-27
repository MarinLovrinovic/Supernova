using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bumper : MonoBehaviour
{
    public float bumpFactor = 0.05f;

    private Rigidbody2D thisBody;

    void Start()
    {
        thisBody = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bumpee")) return;

        Vector2 direction = (other.transform.position - transform.position).normalized;
        Vector2 currVelocity = thisBody.velocity;

        thisBody.velocity = Vector2.zero;
        thisBody.angularVelocity = 0.0f;

        transform.position -= (Vector3)(direction * 0.05f);

        thisBody.AddForce(-currVelocity * bumpFactor, ForceMode2D.Impulse);
    }
}

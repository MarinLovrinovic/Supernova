using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject owner;
    public float speed = 10f;
    public float lifeTime = 10f;
    public float damageAmount = 10.0f;
    public bool damageOnTrigger = true;
    
    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        body.velocity = transform.up * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOwnedBy(other.gameObject)) return;

        if (damageOnTrigger)
        {
            if (other.gameObject.GetComponent<Health>() != null)
            {
                other.gameObject.GetComponent<Health>().ApplyDamage(damageAmount);
            }
        }

        Destroy(gameObject);
    }

    private bool IsOwnedBy(GameObject other)
    {
        if (other == null || owner == null) return true;
        if (other == owner || other.transform.IsChildOf(owner.transform)) return true;

        return false;
    }
    
}

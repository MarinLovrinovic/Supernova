using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifeTime = 10f;
    public GameObject[] respawns;
    private Rigidbody2D weapon;
    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
        respawns = GameObject.FindGameObjectsWithTag("Target");
        
        if (respawns.Length == 0)
        {
            // Debug.Log("No GameObjects are tagged with 'Enemy'");
        }
        else
        {
            // Debug.Log("GameObjects are tagged with 'Enemy'");
        }
        
    }

    private void FixedUpdate()
    {
        weapon.velocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Debug.Log("OnCollisionEnter2D triggered with " + other.gameObject.name);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    
}

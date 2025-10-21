using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifeTime = 10f;
    
    private Rigidbody2D weapon;
    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        weapon.velocity = transform.up * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    
}

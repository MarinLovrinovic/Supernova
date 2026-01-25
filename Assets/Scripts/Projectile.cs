using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class Projectile : NetworkBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifeTime = 1f;
    public float damageAmount = 10f;

    [SerializeField] private NetworkPrefabRef explosionPrefab;
    [SerializeField] private int explosionPrefabIndex = 0;
    [SerializeField] private float explosionRadius = 1.0f;
    [SerializeField] private float explosionDamage;
    
    [Networked] private TickTimer currentLifeTime { get; set; }
    [Networked] public PlayerRef projectileOwner { get; set; }
    public override void Spawned()
    {
        if (Object.HasStateAuthority == false) return;

        currentLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);

        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = transform.up * speed;
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (currentLifeTime.Expired(Runner))
        {
            Explode();
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // had to add this due to NullReferenceException
        if (Object == null || !Object.IsValid)
            return;
        
        if (!Object.HasStateAuthority)
            return;

        if (other.TryGetComponent(out NetworkObject netObj) && netObj.InputAuthority == projectileOwner)
            return;

        if (other.TryGetComponent(out Health health) && health.IsAlive)
        {
            health.ApplyDamage(damageAmount);
        }

        Explode();
        Runner.Despawn(Object);
    }

    private void Explode()
    {
        if (!Object.HasStateAuthority)
            return;
        
        Runner.Spawn(
            explosionPrefab,
            transform.position,
            Quaternion.identity,
            PlayerRef.None,
            (runner, o) => o.transform.localScale = Vector3.one * explosionRadius * 2.0f);

        if (explosionDamage == 0)
            return;
        
        Vector2 explosionCenter = transform.position;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.isTrigger)
                continue;
            
            if (hit.TryGetComponent(out NetworkObject netObj) && netObj.InputAuthority == projectileOwner)
                continue;

            if (!hit.TryGetComponent(out Health health) || !health.IsAlive)
                continue;

            health.ApplyDamage(explosionDamage);
            Debug.Log($"Explosion hit {hit.gameObject.name}");
        }
    }
}
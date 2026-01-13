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
    public bool isExplosive = false;
    [SerializeField] private NetworkPrefabRef explosionPrefab;
    [SerializeField] private float explosionRadius = 1.0f;
    [SerializeField] private float explosionDamage = 1.0f;
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
        CheckLifetime();
    }

    private void CheckLifetime()
    {
        if (!currentLifeTime.Expired(Runner)) return;

        Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority) 
            return;

        if (!other.CompareTag("Player") && !other.CompareTag("Bumpee"))
            return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.InputAuthority == projectileOwner) 
            return;

        var health = other.GetComponent<Health>();
        if (health != null && health.isAlive)
        {
            health.ApplyDamage(damageAmount);
        }

        if (isExplosive) explode();

        Runner.Despawn(Object);
    }

    private void explode()
    {
        if (!Object.HasStateAuthority)
            return;

        Vector2 explosionCenter = transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player") && !hit.CompareTag("Bumpee"))
                continue;

            var netObj = hit.GetComponent<NetworkObject>();
            if (netObj != null && netObj.InputAuthority == projectileOwner)
                continue;

            var health = hit.GetComponent<Health>();
            if (health == null || !health.isAlive)
                continue;

            health.ApplyDamage(explosionDamage);
        }

        if (explosionPrefab.IsValid)
        {
            NetworkObject explosion = Runner.Spawn(explosionPrefab, explosionCenter, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionRadius * 2.0f;
        }
    }
}

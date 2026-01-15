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
    public GameObject[] respawns;
    public float damageAmount = 10f;
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

        if (!other.CompareTag("Player"))
            return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.InputAuthority == projectileOwner)
            return;

        var health = other.GetComponent<Health>();
        if (health != null && health.IsAlive)
        {
            health.ApplyDamage(damageAmount);
        }

        Runner.Despawn(Object);
    }
}
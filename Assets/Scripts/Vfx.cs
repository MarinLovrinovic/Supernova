using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class Vfx : NetworkBehaviour
{
    [SerializeField] float lifeTime = 1f;
    [Networked] private TickTimer currentLifeTime { get; set; }
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority == false) return;

        currentLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }
    public override void FixedUpdateNetwork()
    {
        if (currentLifeTime.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}
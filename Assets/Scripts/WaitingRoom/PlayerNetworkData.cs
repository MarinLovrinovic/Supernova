using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fusion.Addons.Physics;

public class PlayerNetworkData : NetworkBehaviour
{
    // paleta je sad na prefabu da igraci ne ovise o customization da bi se obojali
    [SerializeField] public List<Color> palette;
    [Networked, OnChangedRender(nameof(OnColorChanged))]
    public int ColorIndex { get; set; }
    [SerializeField] public List<Sprite> bodySprite;
    [Networked, OnChangedRender(nameof(OnBodyChanged))]
    public BodyType BodyType { get; set; }
    
    [Networked] public ShieldType Shield { get; set; }
    
    [Networked] public bool IsReady { get; set; }

    public static PlayerNetworkData Local;

    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
        IsReady = false;

        if (Object.HasStateAuthority && ColorIndex == -1 && WaitingRoomManager.Instance)
        {
            ColorIndex = WaitingRoomManager.Instance.GetFirstFreeColor();
        }

        ApplyColor();
        Debug.Log("[PlayerNetworkData.Spawned] Player spawned with color index: " + ColorIndex);

        if (Object.HasInputAuthority)
        {
            Runner.SetPlayerObject(Object.InputAuthority, Object);
            Local = this;
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestColorChange(int index)
    {
        if (Object.HasStateAuthority)
            ColorIndex = index;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RefreshCustomizationUI()
    {
        Customization.Instance.RefreshUI();
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ClickedAccept()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.ServerRegisterAccept(Object.InputAuthority); 
    }

    void OnColorChanged()
    {
        ApplyColor();
    }

    void ApplyColor()
    {
        if (ColorIndex == -1)
            return;

        var renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.color = palette[ColorIndex];

        // svima se updejtaju dostupne boje odma
        if (Object.HasInputAuthority && Customization.Instance != null)
        {
            RPC_RefreshCustomizationUI();
        }
    }
    public void SetReady(bool ready)
    {
        if (Object.HasInputAuthority)
            IsReady = ready;
    }
    
    void OnBodyChanged()
    {
        // basic body stats
        int spriteIndex = 0;
        int maxHealth = 20;
        int acceleration = 4;
        
        if (BodyType == BodyType.Heavy)
        {
            spriteIndex = 1;
            maxHealth = 30;
            acceleration = 2;
        }
        else if (BodyType == BodyType.Light)
        {
            spriteIndex = 2;
            maxHealth = 10;
            acceleration = 6;
        }
        GetComponent<SpriteRenderer>().sprite = bodySprite[spriteIndex];
        var health = GetComponent<Health>();
        health.maxHealth = maxHealth;
        health.HealthPoints = maxHealth;
        GetComponent<PlayerMovement>().thrustForceAcceleration = acceleration;
        
        // add and remove trigger collider so that hitbox adjusts to sprite
        PolygonCollider2D oldCollider = GetComponents<PolygonCollider2D>().First(x => x.isTrigger);
        if (oldCollider)
        {
            Destroy(oldCollider);
        }
        gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
    }
}

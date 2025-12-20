using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Addons.Physics;

public class PlayerNetworkData : NetworkBehaviour
{
    [SerializeField] public List<Color> palette;
    [Networked, OnChangedRender(nameof(OnColorChanged))]
    public int ColorIndex { get; set; }
    [Networked] public bool IsReady { get; set; }

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

}

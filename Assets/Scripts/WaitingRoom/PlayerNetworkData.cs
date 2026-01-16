using Fusion;
using UnityEngine;
using System.Collections.Generic;
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

        if (Object.HasInputAuthority)
        {
            Runner.SetPlayerObject(Object.InputAuthority, Object);
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



    // trebalo bi u upgrade shopu kada se treba promijenit tijelo napravit
    // slicno ka u customization kada se odabere boja da se posalje rpc request hostu 
    // i da host onda u biti promijeni vrijednost BodyType odredenog igraca
    // i onda se igracu ovdi promijeni tijelo iz spriteova koji su spremljeni na svakom igracu
    void OnBodyChanged()
    {
        if (BodyType == BodyType.Basic)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = bodySprite[0];
        }
        else if (BodyType == BodyType.Heavy)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = bodySprite[1];
        }
        else if (BodyType == BodyType.Light)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = bodySprite[2];
        }
    }
}

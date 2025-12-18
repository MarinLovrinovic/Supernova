using Fusion;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;


// ovim upravlja Customization basically
public class PlayerNetworkData : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnColorChanged))] public int ColorIndex { get; set; }
    [Networked] public bool IsReady { get; set; }

    
    public override void Spawned()
    {
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.color = Customization.getColor(ColorIndex);    // boja koju dobije kad se spoji
        Debug.Log("[PlayerNetworkData.Spawned] Player spawned with color index: " + ColorIndex);
    }

     public void SetColor(int index)
    {
        if (index < -1 || index >= Customization.Instance.palette.Count)
            return;

        if (Object.HasInputAuthority)
            ColorIndex = index;
    }

    public void SetReady(bool ready)
    {
        if (Object.HasInputAuthority)
            IsReady = ready;
    }

    
    public void OnColorChanged()
    {
        Debug.Log("[PlayerNetworkData.OnColorChanged] Player color changed to index: " + ColorIndex);
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.color = Customization.getColor(ColorIndex);
    }
}

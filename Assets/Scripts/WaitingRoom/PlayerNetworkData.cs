using Fusion;


public class PlayerNetworkData : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(SetColor))] public int ColorIndex { get; set; }
    [Networked] public bool IsReady { get; set; }

     public void SetColor(int index)
    {
        if (Object.HasInputAuthority)
            ColorIndex = index;
    }

    public void SetReady(bool ready)
    {
        if (Object.HasInputAuthority)
            IsReady = ready;
    }
}

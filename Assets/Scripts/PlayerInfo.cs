using Fusion;
using TMPro;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(UpdatePlayerName))]
    public NetworkString<_8> PlayerName { get; set; }

    [SerializeField] TMP_Text playerNameLabel;

    public override void Spawned()
    {
        UpdatePlayerName();

        if (HasInputAuthority)
        {
            RPC_SetPlayerName(BasicSpawner.instance._playerName);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }

    void UpdatePlayerName()
    {
        if (playerNameLabel != null)
            playerNameLabel.text = PlayerName.ToString();
    }
}

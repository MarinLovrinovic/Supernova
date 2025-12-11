using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;



public class WaitingRoomManager : NetworkBehaviour
{
    private static WaitingRoomManager _instance;
    public static WaitingRoomManager Instance => _instance;
    public PlayerNetworkData LocalPlayer { get; private set; }

    public List<Color> availableColors;
    [Networked] public NetworkArray<int> playerColors => default; // index by PlayerRef.RawEncoded
    
    [Networked] public int timeRemaining { get; private set; }


    public override void Spawned()
    {
        _instance = this;

    }

    public void AddPlayer(PlayerRef player)
    {
        // Assign default color
        AssignFreeColor(player);
    }



    public void ChangePlayerColor(int index)
    {
        LocalPlayer.SetColor(index);
    }
    public void SetPlayerReady()
    {
        LocalPlayer.SetReady(!LocalPlayer.IsReady);
    }



    public bool TryAssignColor(PlayerRef player, int colorIndex)
    {
        // check if taken
        for (int i = 0; i < playerColors.Length; i++)
        {
            if (playerColors[i] == colorIndex)
                return false; // already taken
        }

        // not taken → assign
        playerColors[player.RawEncoded] = colorIndex;
        return true;
    }
    public bool AssignFreeColor(PlayerRef player)
    {
        // check if taken
        for (int i = 0; i < playerColors.Length; i++)
        {
            if (playerColors[i] == colorIndex)
                return false; // already taken
        }

        // not taken → assign
        playerColors[player.RawEncoded] = colorIndex;
        return true;
    }


    public IEnumerator CountdownTick()
    {
        if (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining--;
            WaitingRoomUIManager.Instance.UpdateTimer(timeRemaining);
        }
    }
}

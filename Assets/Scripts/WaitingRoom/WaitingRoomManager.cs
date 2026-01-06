using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fusion.Addons.Physics;




public class WaitingRoomManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    [SerializeField] private NetworkPrefabRef _waitingRoomManagerPrefab;
    public static WaitingRoomManager Instance;
    public PlayerNetworkData LocalPlayer { get; private set; }

    [Networked, Capacity(10), OnChangedRender(nameof(OnPlayersChanged))]
    public NetworkLinkedList<NetworkBehaviourId> Players => default;

    public void OnPlayersChanged()
    {
        WaitingRoomUIManager.Instance.UpdatePlayersJoined(Players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);
    }

    [Networked, OnChangedRender(nameof(OnTimeChanged))]
    public int TimeRemaining { get; private set; }
    private float _timerAcc = 0f;
    private int ReadyCount => ReadyPlayers.Count(kvp => kvp.Value);
    
    [Networked, Capacity(10), OnChangedRender(nameof(OnReadyChanged))]
    public NetworkDictionary<PlayerRef, bool> ReadyPlayers => default;
    public void OnReadyChanged()
    {
        Debug.Log("Ready changed!");
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ToggleReady(PlayerRef player)
    {
        bool isReady = false;

        if (ReadyPlayers.TryGet(player, out var current))
            isReady = current;

        ReadyPlayers.Set(player, !isReady);
        
        if (ReadyCount == Players.Count)
        {
            StartBattleScene();
        }
    }
    private const int MaxPlayers = 6;


    public override void Spawned()
    {
        Instance = this;

        var roomCode = Runner.SessionInfo.Name;
        Debug.Log("[WRM.Spawned] Room code: " + roomCode);

        // inicijalizacija spaceship spawnera
        var spawner = FindObjectOfType<SpaceshipSpawner>();
        Runner.AddCallbacks(spawner);
        spawner.Initialize(GameScene.WaitingRoom, this);

        foreach (var player in Runner.ActivePlayers)
        {
            if (!Runner.TryGetPlayerObject(player, out var playerObj))
                continue;

            var playerData = playerObj.GetComponent<PlayerNetworkData>();

            var nrb = playerObj.GetComponent<NetworkRigidbody2D>();
            nrb.InterpolationTarget = null;

            // svaki igrac postavi svoj lokalni PlayerNetworkData kad se spawna
            if (player == Runner.LocalPlayer)
            {
                LocalPlayer = playerData;
            }

            AddPlayer(playerData.Id);
        }


        if (Object.HasStateAuthority)
        {
            TimeRemaining = 180; // 3 minute i onda pocinje igra
            _timerAcc = 0f;
        }

        RefreshUI();
    }

    public void OnPlayerSpawned(NetworkObject networkPlayerObject, PlayerRef player)
    {
        var data = networkPlayerObject.GetComponent<PlayerNetworkData>();
        if (player == Runner.LocalPlayer)
        {
            SetLocalPlayer(data);
        }

        var playerNetworkData = networkPlayerObject.GetComponent<PlayerNetworkData>();
        AddPlayer(playerNetworkData.Id);
    }

    public void SetLocalPlayer(PlayerNetworkData player)
    {
        Debug.Log("local player set: " + player.Id);
        LocalPlayer = player;

        if (Customization.Instance != null)
            Customization.Instance.RefreshUI();
    }


    public override void FixedUpdateNetwork()
    {
        // timer
        if (Object.HasStateAuthority && TimeRemaining > 0)
        {
            _timerAcc += Runner.DeltaTime;

            if (_timerAcc >= 1f)
            {
                _timerAcc -= 1f;
                TimeRemaining--;
            }
        }
    }

    // funkcije za WaitingRoomUIManager
    public void OnTimeChanged()
    {
        WaitingRoomUIManager.Instance.UpdateTimer(TimeRemaining);
    }
    public void SetPlayerReady()
    {
        if (LocalPlayer == null)
            return;

        RPC_ToggleReady(Runner.LocalPlayer);
    }
    public void AddPlayer(NetworkBehaviourId playerNetworkDataId)
    {
        Debug.Log("player joined " + playerNetworkDataId);

        if (!Object.HasStateAuthority)
            return;

        if (!Players.Contains(playerNetworkDataId))
        {
            Players.Add(playerNetworkDataId);
        }
    }

    // pomocna funkcija za assign boje pri ulasku
    public int GetFirstFreeColor()
    {
        for (int i = 0; i < Customization.Instance.colorButtons.Count; i++)
        {
            bool taken = false;

            foreach (var player in Runner.ActivePlayers)
            {
                if (!Runner.TryGetPlayerObject(player, out var playerObj)) continue;
                if (playerObj.GetComponent<PlayerNetworkData>().ColorIndex == i)
                {
                    taken = true;
                    break;
                }
            }

            if (!taken)
                return i;
        }
        return 0;
    }

    private void RefreshUI()
    {
        var roomCode = Runner.SessionInfo.Name;
        WaitingRoomUIManager.Instance.SetRoomCode(roomCode);
        WaitingRoomUIManager.Instance.UpdateTimer(TimeRemaining);
        WaitingRoomUIManager.Instance.UpdatePlayersJoined(Players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);

        if (Customization.Instance != null)
            Customization.Instance.RefreshUI();
    }




    // privremeno za testiranje 
    public void StartBattleScene()
    {
        if (Object.HasStateAuthority)
        {
            Runner.LoadScene("BattleScene");
        }
    }

}


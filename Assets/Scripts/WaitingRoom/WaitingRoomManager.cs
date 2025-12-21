using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Addons.Physics;




public class WaitingRoomManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    [SerializeField] private NetworkPrefabRef _waitingRoomManagerPrefab;
    public static WaitingRoomManager Instance;
    public PlayerNetworkData LocalPlayer { get; private set; }

    private readonly List<NetworkBehaviourId> players = new();


    [Networked, OnChangedRender(nameof(OnTimeChanged))]
    public int TimeRemaining { get; private set; }
    private float _timerAcc = 0f;
    private int readyCount = 0;
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

            players.Add(playerData.Id);
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

        LocalPlayer.SetReady(!LocalPlayer.IsReady);

        if (LocalPlayer.IsReady)
        {
            readyCount++;

            // bez postavljenog InterpolationTarget na NetworkRigidbody2D dode do errora kad se prijede u Battle Scene
            // ali klijenti se na svojim ekranima ne pomicu pravilno kad je postavljen
            // nasilno rjesenje: kad klijent klikne ready ukljuci mu se i onda se opet iskljuci u battle
            // ako ne nademo bolje rjesenje mozemo dodat neki loading screen za ovo
            // HOST ZADNJI MORA READYAT (privremeno)
            foreach (var player in Runner.ActivePlayers)
            {
                if (!Runner.TryGetPlayerObject(player, out var playerObject))
                    continue;

                var nrb = playerObject.GetComponent<NetworkRigidbody2D>();
                nrb.InterpolationTarget = nrb.GetComponentInChildren<InterpolationTarget>().Target;
            }

            StartBattleScene();
        }
        else
        {
            readyCount--;
        }

        WaitingRoomUIManager.Instance.UpdatePlayersReady(readyCount, players.Count);


    }
    public void AddPlayer(NetworkBehaviourId playerNetworkDataId)
    {
        Debug.Log("player joined " + playerNetworkDataId);
        players.Add(playerNetworkDataId); // manager ima uvid na sve igrace koji udu 


        WaitingRoomUIManager.Instance.UpdatePlayersJoined(players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(readyCount, players.Count);
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
        WaitingRoomUIManager.Instance.UpdatePlayersJoined(players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(readyCount, players.Count);

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


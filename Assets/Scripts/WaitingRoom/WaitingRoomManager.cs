using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaitingRoomManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    public static WaitingRoomManager Instance;

    public PlayerNetworkData LocalPlayer { get; private set; }

    [Networked, Capacity(10), OnChangedRender(nameof(OnPlayersChanged))]
    public NetworkLinkedList<NetworkBehaviourId> Players => default;

    [Networked, OnChangedRender(nameof(OnTimeChanged))]
    public int TimeRemaining { get; private set; }

    [Networked, Capacity(10), OnChangedRender(nameof(OnReadyChanged))]
    public NetworkDictionary<PlayerRef, bool> ReadyPlayers => default;

    [Networked, OnChangedRender(nameof(OnGameStartedChanged))]
    public NetworkBool GameStarted { get; set; }

    public GameObject DisableBeforeBattleScene;
    private const int MaxPlayers = 6;
    private float _timerAcc = 0f;
    private int ReadyCount => ReadyPlayers.Count(kvp => kvp.Value);

    public override void Spawned()
    {
        Instance = this;

        var spawner = FindObjectOfType<SpaceshipSpawner>();
        if (spawner)
        {
            Runner.AddCallbacks(spawner);
            spawner.Initialize(GameScene.WaitingRoom, this);
        }

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                var data = playerObj.GetComponent<PlayerNetworkData>();

                if (player == Runner.LocalPlayer)
                {
                    SetLocalPlayer(data);
                }

                AddPlayer(data.Id);
            }
        }

        if (Object.HasStateAuthority)
        {
            TimeRemaining = 180;
            GameStarted = false;
        }

        RefreshUI();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && TimeRemaining > 0 && !GameStarted)
        {
            _timerAcc += Runner.DeltaTime;
            if (_timerAcc >= 1f)
            {
                _timerAcc -= 1f;
                TimeRemaining--;
            }
        }
    }
    public void SetLocalPlayer(PlayerNetworkData player)
    {
        Debug.Log("Local player set: " + player.Id);
        LocalPlayer = player;

        if (Customization.Instance != null)
            Customization.Instance.RefreshUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ToggleReady(PlayerRef player)
    {
        bool isReady = false;
        if (ReadyPlayers.TryGet(player, out var current)) isReady = current;

        ReadyPlayers.Set(player, !isReady);

        if (ReadyCount == Players.Count && Players.Count > 0)
        {
            StartBattleScene();
        }
    }

    public void StartBattleScene()
    {
        if (Object.HasStateAuthority)
        {
            GameStarted = true;
            Runner.LoadScene("BattleScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }

    public void OnGameStartedChanged()
    {
        if (DisableBeforeBattleScene != null)
            DisableBeforeBattleScene.SetActive(!GameStarted);
    }

    public void OnPlayersChanged()
    {
        WaitingRoomUIManager.Instance.UpdatePlayersJoined(Players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);
    }

    public void OnReadyChanged()
    {
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);
    }

    public void OnTimeChanged()
    {
        WaitingRoomUIManager.Instance.UpdateTimer(TimeRemaining);
    }

    public void SetPlayerReady()
    {
        if (LocalPlayer != null)
        {
            RPC_ToggleReady(Runner.LocalPlayer);
        }
    }

    public void AddPlayer(NetworkBehaviourId id)
    {
        if (Object.HasStateAuthority && !Players.Contains(id))
        {
            Players.Add(id);
        }
    }

    public void OnPlayerSpawned(NetworkObject networkPlayerObject, PlayerRef player)
    {
        var data = networkPlayerObject.GetComponent<PlayerNetworkData>();

        if (player == Runner.LocalPlayer)
        {
            SetLocalPlayer(data);
        }

        AddPlayer(data.Id);
    }

    private void RefreshUI()
    {
        if (WaitingRoomUIManager.Instance == null) return;

        WaitingRoomUIManager.Instance.SetRoomCode(Runner.SessionInfo.Name);
        WaitingRoomUIManager.Instance.UpdateTimer(TimeRemaining);
        WaitingRoomUIManager.Instance.UpdatePlayersJoined(Players.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(ReadyCount, Players.Count);

        if (Customization.Instance != null)
            Customization.Instance.RefreshUI();
    }

    public int GetFirstFreeColor()
    {
        if (Customization.Instance == null) return 0;

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
}
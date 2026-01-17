using DefaultNamespace;
using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    public static BattleManager Instance;

    [Header("Settings")]
    [SerializeField] private string upgradeShopSceneName = "UpgradeShop";
    // [SerializeField] private string afterShopScene = "WaitingRoom";
    // [SerializeField] private string winnerSceneName = "WinnerScene";
    [SerializeField] private int nextRoundCountdownSeconds = 10;
    [SerializeField] public NetworkPrefabRef defaultWeaponPrefab;

    private bool _battleEnded = false;
    private bool _winnerPopupOpened = false;
    
    [Networked] public NetworkDictionary<PlayerRef, NetworkBool> AcceptedPlayers => default;
    [Networked] public int AcceptedCount { get; set; }
    [Networked] public int AcceptRequired { get; set; }
    [Networked] public TickTimer NextRoundTimer { get; set; }
    [Networked] public NetworkBool NextRoundCountdownRunning { get; set; }
    [Networked] public PlayerRef WinnerRef { get; set; }

    public override void Spawned()
    {
        Instance = this;

        var spawner = FindObjectOfType<SpaceshipSpawner>();
        if (spawner)
        {
            Runner.AddCallbacks(spawner);
            spawner.Initialize(GameScene.Battle, this);
        }

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                OnPlayerSpawned(playerObj, player);
            }
        }

        if (Object.HasStateAuthority)
        {
            AsteroidSpawner.Instance.SpawnAsteroids(Runner);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return; 

        if (!_battleEnded) 
        {
            CheckWinCondition();
            return; 
        }
        
        if (NextRoundCountdownRunning && NextRoundTimer.Expired(Runner)) 
        {
            StartNewRound(); 
        }
    }

    private void CheckWinCondition()
    {
        int aliveCount = 0;
        PlayerRef lastAliveRef = default;
        PlayerNetworkData lastAlivePlayer = null;

        foreach (var playerRef in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerObj))
            {
                var health = playerObj.GetComponent<Health>();
                if (health != null && health.IsAlive)
                {
                    aliveCount++;
                    lastAliveRef = playerRef;
                    lastAlivePlayer = playerObj.GetComponent<PlayerNetworkData>();
                }
            }
        }

        if (aliveCount <= 1)
        {
            if (!_battleEnded)//TODO
                EndRound(lastAlivePlayer, lastAliveRef);
        }
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        Debug.Log($"Player {playerRef} died.");
    }

    private void EndRound(PlayerNetworkData winner, PlayerRef winnerRef)
    {
        _battleEnded = true;
        Debug.Log("Round Ended!");

        if (winner != null)
        {
            Debug.Log($"Winner is {winner.Id}");
        }

        AsteroidSpawner.Instance.DespawnAsteroids(Runner);
        
        WinnerRef = winnerRef;
        if (AcceptedPlayers.ContainsKey(WinnerRef))
        {
            AcceptedPlayers.Remove(WinnerRef);
        }
        
        AcceptRequired = Mathf.Max(0, Runner.ActivePlayers.Count() - 1);
        NextRoundCountdownRunning = false;
        
        if (!_winnerPopupOpened)
        {
            _winnerPopupOpened = true;
            RPC_OpenWinnerPopup(winnerRef); 
        }
        
        if (!NextRoundCountdownRunning && AcceptedCount >= AcceptRequired) 
        {
            NextRoundCountdownRunning = true; 
            NextRoundTimer = TickTimer.CreateFromSeconds(Runner, nextRoundCountdownSeconds); 
            RPC_NextRoundCountdownStarted(); 
        }
        
    }


    public void ServerRegisterAccept(PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;

        if (AcceptRequired <= 0 && Runner != null)
        {
            AcceptRequired = Mathf.Max(0, Runner.ActivePlayers.Count() - 1);
        }
            
        if (WinnerRef != default && player == WinnerRef) return;
        
        if (!AcceptedPlayers.ContainsKey(player))
            AcceptedPlayers.Add(player, true);
        
        int count = 0;
        foreach (var kv in AcceptedPlayers)
            if (kv.Value) count++;

        AcceptedCount = count;

        if (_battleEnded && !NextRoundCountdownRunning && AcceptedCount >= AcceptRequired)
        {
            NextRoundCountdownRunning = true;
            NextRoundTimer = TickTimer.CreateFromSeconds(Runner, nextRoundCountdownSeconds);
            RPC_NextRoundCountdownStarted();
        }
        
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NextRoundCountdownStarted()
    {
        if (Runner == null) return;

        // pause only locally (same idea as shop/winner popup)
        Runner.ProvideInput = false;
    }
    
    public float GetNextRoundRemaining()
    {
        if (Runner == null) return -1f;
        if (!NextRoundCountdownRunning) return -1f;

        float t = NextRoundTimer.RemainingTime(Runner) ?? -1f;
        return t;
    }

    private void StartNewRound()
    {
        _battleEnded = false;
        _winnerPopupOpened = false;

        AcceptedCount = 0;
        AcceptRequired = 0;
        AcceptedPlayers.Clear();
        NextRoundCountdownRunning = false;
        WinnerRef = default;
        
        foreach (var playerRef in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerObj))
            {
                var health = playerObj.GetComponent<Health>();
                if (health)
                {
                    health.IsAlive = true;
                    health.HealthPoints = health.maxHealth;
                }
            }
        }
        
        AsteroidSpawner.Instance.DespawnAsteroids(Runner);
        AsteroidSpawner.Instance.SpawnAsteroids(Runner);
        RPC_StartNewRoundClient();
        
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartNewRoundClient()
    {
        if (Runner == null) return;

        Runner.ProvideInput = true;
        
        var shopScene = SceneManager.GetSceneByName(upgradeShopSceneName); 
        if (shopScene.IsValid() && shopScene.isLoaded) 
            SceneManager.UnloadSceneAsync(upgradeShopSceneName);
        
    }
    

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OpenWinnerPopup(PlayerRef winnerRef) 
    {
        if (Runner == null) return;
        if (Runner.LocalPlayer != winnerRef) return;

        Runner.ProvideInput = false;
        SceneManager.LoadSceneAsync(upgradeShopSceneName, LoadSceneMode.Additive);
    }

    
    
    public void OnPlayerSpawned(NetworkObject networkPlayerObject, PlayerRef player)
    {
        var data = networkPlayerObject.GetComponent<PlayerNetworkData>();

        var nrb = networkPlayerObject.GetComponent<NetworkRigidbody2D>();
        if (nrb) nrb.InterpolationTarget = null;

        if (Object.HasStateAuthority)
        {
            if (networkPlayerObject.GetComponentInChildren<PlayerWeapon>() == null)
            {
                NetworkObject weapon = Runner.Spawn(
                    defaultWeaponPrefab,
                    networkPlayerObject.transform.position,
                    networkPlayerObject.transform.rotation,
                    player
                );
                weapon.transform.SetParent(networkPlayerObject.transform, false);
            }
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitUpgrade(PlayerRef player, Upgrades upgrade)
    {
        Debug.Log($"Upgrade submitted: {player} -> {upgrade}");
    }
}
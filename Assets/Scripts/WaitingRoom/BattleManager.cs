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
    [SerializeField] private int nextRoundCountdownSeconds = 10;
    
    [SerializeField] public NetworkPrefabRef defaultWeaponPrefab;
    [SerializeField] private NetworkPrefabRef raygunPrefab;          
    [SerializeField] private NetworkPrefabRef bigRaygunPrefab;       
    [SerializeField] private NetworkPrefabRef rocketPrefab;          
    [SerializeField] private NetworkPrefabRef bigRocketPrefab; 
    [SerializeField] private NetworkPrefabRef smallShieldPrefab;          
    [SerializeField] private NetworkPrefabRef bigShieldPrefab;
    
    
    private bool _battleEnded = false;
    private bool _winnerPopupOpened = false;
    
    [Networked] public NetworkDictionary<PlayerRef, NetworkBool> AcceptedPlayers => default;
    [Networked] public NetworkDictionary<PlayerRef, Upgrades> PlayerUpgrades => default;
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
            Debug.Log("player: " + playerRef);

            if (Runner.TryGetPlayerObject(playerRef, out var playerObj))
            {
                var health = playerObj.GetComponent<Health>();
                if (health)
                {
                    health.IsAlive = true;
                    health.HealthPoints = health.maxHealth;
                }
                
                if (PlayerUpgrades.ContainsKey(playerRef)) 
                {
                    ApplyUpgradeToPlayer(playerObj, PlayerUpgrades[playerRef]);
                    PlayerUpgrades.Remove(playerRef);
                }
            }
        }
        
        AsteroidSpawner.Instance.DespawnAsteroids(Runner);
        AsteroidSpawner.Instance.SpawnAsteroids(Runner);
        RPC_StartNewRoundClient();
        
    }
    
    
    private void ApplyUpgradeToPlayer(NetworkObject playerObj, Upgrades upgrade)
    {
        var player = playerObj.GetComponent<PlayerNetworkData>();
        var weapon = playerObj.GetComponentInChildren<PlayerWeapon>();

        if (player == null) return;

        switch (upgrade)
        {
            case Upgrades.Shield:
                if (player.Shield == ShieldType.None)
                {
                    player.Shield = ShieldType.Small;
                    AddShield();
                }
                else if (player.Shield == ShieldType.Small)
                {
                    player.Shield = ShieldType.Large;
                    UpgradeShield();
                }
                else
                {
                    UpgradeShield();
                }
                break;
            
            case Upgrades.Rockets:
                ReplaceWeapon(playerObj, WeaponType.RocketLauncher);
                break;
            
            case Upgrades.Spaceship:
            {
                BodyType newType;
                do
                {
                    newType = (BodyType)Random.Range(0, 3);
                }
                while (newType == player.BodyType); 

                player.BodyType = newType;
                break;
            }
            
            case Upgrades.Sword:
                if (weapon == null) break;

                if (weapon.CurrentWeapon == WeaponType.Raygun)
                    ReplaceWeapon(playerObj, WeaponType.BigRaygun);
                else if (weapon.CurrentWeapon == WeaponType.RocketLauncher)
                    ReplaceWeapon(playerObj, WeaponType.BigRocketLauncher);
                break;
            
            
            default:
                Debug.Log("Upgrade not implemented: " + upgrade);
                break;
        }
    }

    private void AddShield()
    {
        
    }

    private void UpgradeShield()
    {

    }


    private void ReplaceWeapon(NetworkObject playerObj, WeaponType newType)
    {
        if (!Object.HasStateAuthority) return;

        var oldWeapon = playerObj.GetComponentInChildren<PlayerWeapon>();
        if (oldWeapon != null)
            Runner.Despawn(oldWeapon.Object);
        
        NetworkPrefabRef prefab = newType switch
        {
            WeaponType.Raygun => raygunPrefab,
            WeaponType.BigRaygun => bigRaygunPrefab,
            WeaponType.RocketLauncher => rocketPrefab,
            WeaponType.BigRocketLauncher => bigRocketPrefab,
            _ => raygunPrefab
        };
        
        var weaponObj = Runner.Spawn(
            prefab,
            playerObj.transform.position,
            playerObj.transform.rotation,
            playerObj.InputAuthority
        );

        weaponObj.transform.SetParent(playerObj.transform, false);

        var playerWeapon = weaponObj.GetComponent<PlayerWeapon>();
        if (playerWeapon != null)
            playerWeapon.CurrentWeapon = newType;

        Debug.Log("Set new weapon: " + weaponObj);
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
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitUpgrade(PlayerRef player, Upgrades upgrade)
    {
        Debug.Log($"Upgrade submitted: {player} -> {upgrade}");
        
        if (PlayerUpgrades.ContainsKey(player)) 
            PlayerUpgrades.Remove(player);      

        PlayerUpgrades.Add(player, upgrade);
    }
}
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
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private int nextRoundCountdownSeconds = 10;
    
    [SerializeField] public NetworkPrefabRef defaultWeaponPrefab;
    [SerializeField] private NetworkPrefabRef raygunPrefab;          
    [SerializeField] private NetworkPrefabRef bigRaygunPrefab;       
    [SerializeField] private NetworkPrefabRef rocketLauncherPrefab;          
    [SerializeField] private NetworkPrefabRef bigRocketLauncherPrefab; 
    [SerializeField] private NetworkPrefabRef smallShieldPrefab;          
    [SerializeField] private NetworkPrefabRef bigShieldPrefab;
    
    
    private bool _battleEnded = false;
    private bool _winnerPopupOpened = false;
    private Vector3 smallShieldOffset = new Vector3(0.0f, 2.0f, 0.0f);
    
    [Networked] public NetworkDictionary<PlayerRef, NetworkBool> AcceptedPlayers => default;
    [Networked] public NetworkDictionary<PlayerRef, Upgrades> PlayerUpgrades => default;
    [Networked] public int AcceptedCount { get; set; }
    [Networked] public int AcceptRequired { get; set; }
    [Networked] public TickTimer NextRoundTimer { get; set; }
    [Networked] public NetworkBool NextRoundCountdownRunning { get; set; }
    [Networked] public PlayerRef WinnerRef { get; set; }
    [Networked] public NetworkBool GameOver { get; set; }
    [Networked] public PlayerRef FinalWinner { get; set; }

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
            GameOver = false;
            AsteroidSpawner.Instance.SpawnAsteroids(Runner);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (GameOver) return;

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
                var playerData = playerObj.GetComponent<PlayerNetworkData>();
                if (health != null && health.IsAlive && playerData != null && playerData.Lives > 0)
                {
                    aliveCount++;
                    lastAliveRef = playerRef;
                    lastAlivePlayer = playerData;
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

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                var playerData = playerObj.GetBehaviour<PlayerNetworkData>();
                if (playerData != null && player != winnerRef &&  playerData.Lives > 0)
                {
                    playerData.Lives--;
                    Debug.Log($"Player {player} lost a life. Lives remaining: {playerData.Lives}");
                }
            }
        }

        int playersWithLives = 0;
        PlayerRef finalWinnerRef = default;

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                var playerData = playerObj.GetComponent<PlayerNetworkData>();
                if (playerData != null && playerData.Lives > 0)
                {
                    playersWithLives++;
                    finalWinnerRef = player;
                }
            }
        }

        if (playersWithLives <= 1)
        {
            GameOver = true;
            FinalWinner = finalWinnerRef;
            AsteroidSpawner.Instance.DespawnAsteroids(Runner);
            RPC_ShowGameOver(finalWinnerRef);
            return;
        }

        AsteroidSpawner.Instance.DespawnAsteroids(Runner);
        
        WinnerRef = winnerRef;
        if (AcceptedPlayers.ContainsKey(WinnerRef))
        {
            AcceptedPlayers.Remove(WinnerRef);
        }

        AcceptRequired = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                var playerData = playerObj.GetComponent<PlayerNetworkData>();
                if (playerData != null && playerData.Lives > 0 && player != winnerRef)
                {
                    AcceptRequired++;
                }
            }
        }

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowGameOver(PlayerRef finalWinnerRef)
    {
        if (Runner == null) 
            return;

        Runner.ProvideInput = false;

        var shopScene = SceneManager.GetSceneByName(upgradeShopSceneName);
        if (shopScene.IsValid() && shopScene.isLoaded)
            SceneManager.UnloadSceneAsync(upgradeShopSceneName);

        SceneManager.LoadSceneAsync(gameOverSceneName, LoadSceneMode.Additive);
    }


    public void ServerRegisterAccept(PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;
        if (GameOver) return;

        if (Runner.TryGetPlayerObject(player, out var playerObj))
        {
            var playerData = playerObj.GetComponent<PlayerNetworkData>();
            if (playerData != null && playerData.Lives <= 0)
            {
                Debug.Log("$Player {player} has no lives left, ignoring accept");
                return;
            }
        }

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
                var playerData = playerObj.GetComponent<PlayerNetworkData>();

                if (playerData != null && playerData.Lives > 0)
                {
                    foreach (var health in playerObj.GetComponentsInChildren<Health>())
                    {
                        if (health)
                        {
                            health.IsAlive = true;
                            health.HealthPoints = health.maxHealth;
                        }
                    }
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
                    AddShield(playerObj);
                }
                else if (player.Shield == ShieldType.Small)
                {
                    player.Shield = ShieldType.Large;
                    UpgradeShield(playerObj);
                }
                else
                {
                    UpgradeShield(playerObj);
                }
                break;
            
            case Upgrades.Rockets:
                if (weapon.CurrentWeapon == WeaponType.Raygun)
                    ReplaceWeapon(playerObj, WeaponType.RocketLauncher);
                else if (weapon.CurrentWeapon is WeaponType.BigRaygun or WeaponType.RocketLauncher)
                    ReplaceWeapon(playerObj, WeaponType.BigRocketLauncher);
                break;
            
            case Upgrades.Spaceship:
            {
                player.BodyType = (BodyType)(((int)player.BodyType + Random.Range(1, 3)) % 3);
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

    private void AddShield(NetworkObject playerObj)
    {
        if (!Object.HasStateAuthority) return;

        NetworkPrefabRef prefab = smallShieldPrefab;

        var shieldObj = Runner.Spawn(
            prefab,
            playerObj.transform.position,
            playerObj.transform.rotation,
            playerObj.InputAuthority
        );

        shieldObj.transform.SetParent(playerObj.transform, true);
        shieldObj.transform.localPosition = smallShieldOffset;
    }

    private void UpgradeShield(NetworkObject playerObj)
    {
        if (!Object.HasStateAuthority) return;

        NetworkPrefabRef prefab = bigShieldPrefab;

        var shieldObj = Runner.Spawn(
            prefab,
            playerObj.transform.position,
            playerObj.transform.rotation,
            playerObj.InputAuthority
        );

        shieldObj.transform.SetParent(playerObj.transform, true);
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
            WeaponType.RocketLauncher => rocketLauncherPrefab,
            WeaponType.BigRocketLauncher => bigRocketLauncherPrefab,
            _ => raygunPrefab
        };
        
        var weaponObj = Runner.Spawn(
            prefab,
            null,
            null,
            playerObj.InputAuthority,
            (runner, o) => o.transform.SetParent(playerObj.transform, false)
        );

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

        var shopScene = SceneManager.GetSceneByName(upgradeShopSceneName);
        if (shopScene.IsValid() && shopScene.isLoaded)
        {
            Debug.Log("[BattleManager] Unloading shop scene");
            SceneManager.UnloadSceneAsync(upgradeShopSceneName);
        }

        Runner.ProvideInput = true;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OpenWinnerPopup(PlayerRef winnerRef)
    {
        if (Runner == null) return;

        var shopScene = SceneManager.GetSceneByName(upgradeShopSceneName);
        if (shopScene.IsValid() && shopScene.isLoaded)
        {
            Debug.Log("[BattleManager] Shop already loaded, skipping");
            return;
        }

        if (Runner.TryGetPlayerObject(Runner.LocalPlayer, out var playerObj))
        {
            var playerData = playerObj.GetComponent<PlayerNetworkData>();
            if (playerData != null && playerData.Lives <= 0)
            {
                Debug.Log("[BattleManager] Player eliminated, loading shop in spectator mode");
                Runner.ProvideInput = false;
                SceneManager.LoadSceneAsync(upgradeShopSceneName, LoadSceneMode.Additive);
                return;
            }
        }

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
    private bool IsShopLoadedLocally()
    {
        var shopScene = SceneManager.GetSceneByName(upgradeShopSceneName);
        return shopScene.IsValid() && shopScene.isLoaded;
    }
}
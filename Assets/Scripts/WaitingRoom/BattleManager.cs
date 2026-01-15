using DefaultNamespace;
using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    public static BattleManager Instance;

    [Header("Settings")]
    [SerializeField] private string upgradeShopSceneName = "UpgradeShop";
    [SerializeField] private string afterShopScene = "AfterShopScene";
    [SerializeField] public NetworkPrefabRef defaultWeaponPrefab;

    private bool _battleEnded = false;

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
        if (!Object.HasStateAuthority || _battleEnded) return;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        int aliveCount = 0;
        PlayerNetworkData lastAlivePlayer = null;

        foreach (var playerRef in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerObj))
            {
                var health = playerObj.GetComponent<Health>();
                if (health != null && health.IsAlive)
                {
                    aliveCount++;
                    lastAlivePlayer = playerObj.GetComponent<PlayerNetworkData>();
                }
            }
        }

        if (aliveCount <= 1)
        {
            EndRound(lastAlivePlayer);
        }
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        Debug.Log($"Player {playerRef} died.");
    }

    private void EndRound(PlayerNetworkData winner)
    {
        _battleEnded = true;
        Debug.Log("Round Ended!");

        if (winner != null)
        {
            Debug.Log($"Winner is {winner.Id}");
        }

        AsteroidSpawner.Instance.DespawnAsteroids();
        LoadUpgradeShopScene();
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
    public void LoadUpgradeShopScene()
    {
        if (!Object.HasStateAuthority) return;

        Debug.Log("Loading Shop Scene...");

        Runner.LoadScene(upgradeShopSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void StoreUpgradeAndCloseShop(Upgrades selectedUpgrade)
    {
        if (!Object.HasStateAuthority) return;

        _battleEnded = false;

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

        Runner.LoadScene(afterShopScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
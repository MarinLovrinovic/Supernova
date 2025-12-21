using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Addons.Physics;
using System.Diagnostics.Tracing;




public class BattleManager : NetworkBehaviour, IPlayerSpawnerHandler
{
    [SerializeField] private NetworkPrefabRef _BattleManagerPrefab;
    public static BattleManager Instance;
    public PlayerNetworkData LocalPlayer { get; private set; }

    private readonly List<NetworkBehaviourId> players = new();

    private int readyCount = 0;
    private const int MaxPlayers = 6;


    public override void Spawned()
    {
        Instance = this;


        Debug.Log("[BattleManager] Spawning all active players");
        foreach (var player in Runner.ActivePlayers)
        {
            if (!Runner.TryGetPlayerObject(player, out var playerObj))
                continue;

            var playerData = playerObj.GetComponent<PlayerNetworkData>();


            // iskljucivanje InterpolationTarget da se klijenti mogu micat
            var nrb = playerObj.GetComponent<NetworkRigidbody2D>();
            nrb.InterpolationTarget = null;
        


            if (player == Runner.LocalPlayer)
            {
                LocalPlayer = playerData;
            }

            players.Add(playerData.Id);
        }

        //if (!Runner.IsServer)
        //return;


        // inicijalizacija spaceship spawnera
        var spawner = FindObjectOfType<SpaceshipSpawner>();
        Runner.AddCallbacks(spawner);
        spawner.Initialize(GameScene.Battle, this);

        foreach (var player in Runner.ActivePlayers)
        {
            if (!Runner.TryGetPlayerObject(player, out var obj))
                continue;

            OnPlayerSpawned(obj, player);
        }

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



    public void AddPlayer(NetworkBehaviourId playerNetworkDataId)
    {
        Debug.Log("player joined " + playerNetworkDataId);
        players.Add(playerNetworkDataId); // manager ima uvid na sve igrace koji udu 
    }



}
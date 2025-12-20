using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkInputData;
using UnityEngine.UI;
using Random = System.Random;
using Fusion.Addons.Physics;


public interface IPlayerSpawnerHandler
{
    void OnPlayerSpawned(NetworkObject player, PlayerRef playerRef);
}

public class SpaceshipSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private IPlayerSpawnerHandler _handler;
    private GameScene _scene;



    // ovo zove waiting room manager da ne bi bilo problema s redoslijedom spawnanja
    public void Initialize(GameScene scene, IPlayerSpawnerHandler handler = null)
    {
        _scene = scene;
        _handler = handler;

    }


    // poziva se kad se igrac pridruzi
    private void SpawnSpaceship(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        if (!runner.TryGetPlayerObject(player, out var playerObject))
        {
            playerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            runner.SetPlayerObject(player, playerObject);
        }
        
        _spawnedCharacters.Add(player, playerObject);

        Debug.Log($"[SpaceshipSpawner] Spawned player {player} at {spawnPosition}");


        var nrb = playerObject.GetComponent<NetworkRigidbody2D>();
        if (nrb != null)
        {
            float currentZRotation = playerObject.transform.eulerAngles.z;
            nrb.Teleport(spawnPosition, Quaternion.Euler(0, 0, currentZRotation));
        }




        // zovi handler za scene-specific akcije
        _handler?.OnPlayerSpawned(playerObject, player);
    }


    // da se ne spawnaju svi na isto mjesto
    private Vector3 GetRandomSpawnPosition()
    {
        float range = 10f; // udaljenost od spawner-a
        float x = UnityEngine.Random.Range(-range, range);
        float y = UnityEngine.Random.Range(-range, range);
        return transform.position + new Vector3(x, y, 0);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("player joined");
        if (runner.IsServer)
        {
            SpawnSpaceship(runner, player);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var localInput = new NetworkInputData();
        //Debug.Log("[SpaceshipSpawner.OnInput] ");


        localInput.up = Input.GetKey(KeyCode.UpArrow);
        localInput.down = Input.GetKey(KeyCode.DownArrow);
        localInput.right = Input.GetKey(KeyCode.RightArrow);
        localInput.left = Input.GetKey(KeyCode.LeftArrow);
        //localInput.Buttons.Set(SpaceshipButtons.Fire, Input.GetButton("Jump")); 

        input.Set(localInput);

    }



    public void OnSceneLoadDone(NetworkRunner runner) { }
    private void OnGUI() { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}

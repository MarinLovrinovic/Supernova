using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;
using UnityEngine.UI;



public class WaitingRoomManager : NetworkBehaviour
{
    private static WaitingRoomManager _instance;
    public static WaitingRoomManager Instance => _instance;

    [SerializeField] private NetworkPrefabRef _waitingRoomManagerPrefab;
    public PlayerNetworkData LocalPlayer { get; private set; }
    public List<Color> availableColors;
    [Networked, Capacity(MaxPlayers), OnChangedRender(nameof(OnColorsChanged))]
    public NetworkArray<int> playerColors => default;
    [Networked, OnChangedRender(nameof(OnTimeChanged))] public int timeRemaining { get; private set; }
    private float _countdownAccumulator = 0f;
    private int _readyPlayersCount = 0;
    private bool _initialized = false;
    private const int MaxPlayers = 6;


    private List<NetworkBehaviourId> _playerNetworkDataIds = new List<NetworkBehaviourId>();



    public override void Spawned()
    {
        _instance = this;
        var roomCode = Runner.SessionInfo.Name;
        Debug.Log("Room code: " + roomCode);

        WaitingRoomUIManager.Instance.SetRoomCode(roomCode);


        // inicijalizacija spaceship spawnera
        var spawner = FindObjectOfType<SpaceshipSpawner>();
        Runner.AddCallbacks(spawner);
        spawner.StartSpaceshipSpawner(this);


        timeRemaining = 180; // 3 minute i onda pocinje igra
        _countdownAccumulator = 0f;


        var colors = playerColors; // bez ovog koraka bude greska
        if (Object.HasStateAuthority)
        {
            // reset boja na -1 samo prvi put
            if (!_initialized)
            {
                for (int i = 0; i < playerColors.Length; i++)
                    colors[i] = -1;

                _initialized = true;
            }
        }

        
    }

    public override void FixedUpdateNetwork()
    {
        // timer
        if (Object.HasStateAuthority && timeRemaining > 0)
        {
            _countdownAccumulator += Runner.DeltaTime;

            if (_countdownAccumulator >= 1f)
            {
                _countdownAccumulator -= 1f;
                timeRemaining--;
            }
        }
    }

    public void SetLocalPlayer(PlayerNetworkData player)
    {
        Debug.Log("local player set: " + player.Id);
        LocalPlayer = player;
    }


    // funkcije za WaitingRoomUIManager
    public void OnTimeChanged()
    {
        WaitingRoomUIManager.Instance.UpdateTimer(timeRemaining);
    }
    public void SetPlayerReady()
    {
        if (LocalPlayer == null)
            return;

        LocalPlayer.SetReady(!LocalPlayer.IsReady);

        if (LocalPlayer.IsReady)
            _readyPlayersCount++;
        else
            _readyPlayersCount--;

        WaitingRoomUIManager.Instance.UpdatePlayersReady(_readyPlayersCount, _playerNetworkDataIds.Count);
    }

    public void AddPlayer(NetworkBehaviourId playerNetworkDataId)
    {
        Debug.Log("player joined " + playerNetworkDataId);
        _playerNetworkDataIds.Add(playerNetworkDataId); // manager ima uvid na sve igrace koji udu 


        // assigna pocetnu boju
        Debug.Log("first free color: " + GetFirstFreeColor());
        if (_initialized && LocalPlayer)
            ChangePlayerColor(GetFirstFreeColor());


        WaitingRoomUIManager.Instance.UpdatePlayersJoined(_playerNetworkDataIds.Count, MaxPlayers);
        WaitingRoomUIManager.Instance.UpdatePlayersReady(_readyPlayersCount, _playerNetworkDataIds.Count);
    }


    // ovo poziva i Customization kada player odabere boju
    public void ChangePlayerColor(int index)
    {
        if (LocalPlayer == null)
            return;

        Debug.Log("[WRM.ChangePlayerColor] Changing color to index: " + index);

        if (!Object.HasStateAuthority) // samo server moze mijenjati boje
            return;

        int p = LocalPlayer.Object.InputAuthority.PlayerId;
        var colors = playerColors;
        Debug.Log("[WRM.ChangePlayerColor] Current color of player " + p + " is " + colors[p]);

        if (IsColorTaken(index)) {
            Debug.Log("[WRM.ChangePlayerColor] Color " + index + " is already taken!");
            return;
        }

        colors[p] = index;
        LocalPlayer.SetColor(index);
    }

    public bool IsColorTaken(int index)
    {
        for (int i = 0; i < playerColors.Length; i++)
        {
            //Debug.Log("[WRM.IsColorTaken] Color at " + i + " is " + playerColors[i]);
            if (playerColors[i] == index)
                return true;
        }
        return false;
    }
    private int GetFirstFreeColor()
    {
        for (int i = 0; i < playerColors.Length; i++)
        {
            if (!IsColorTaken(i))
                return i;
        }

        return 0; // ako su sve boje zauzete (nemoguce)
    }


    // za panel u kojem se mijenjaju boje
    public void OnColorsChanged()
    {
        Debug.Log("Colors changed");
        if (Customization.Instance != null)
            Customization.Instance.RefreshUI();
    }


}

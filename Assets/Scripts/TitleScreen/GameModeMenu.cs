using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;
using TMPro;


public class GameModeMenu : MonoBehaviour
{

    private NetworkRunner _runner;
    [SerializeField] private NetworkRunner _networkRunnerPrefab = null;

    [SerializeField] private Button _hostButton = null;
    [SerializeField] private Button _joinButton = null;
    [SerializeField] private TMP_InputField _roomCodeInput = null;


    private void Start()
    {
        _hostButton.onClick.AddListener(() => StartGame(GameMode.Host, "", "WaitingRoom"));
        _joinButton.onClick.AddListener(() => StartGame(GameMode.Client, _roomCodeInput.text.ToUpper(), "WaitingRoom"));
        //_joinButton.onClick.AddListener(() => Debug.Log("[GameModeMenu.Start] Join button clicked with code: " + _roomCodeInput.text));
    }


    async void StartGame(GameMode mode, string code, string sceneName)
    {
        if (mode == GameMode.Host)
        {
            code = GenerateRoomCode();
        }


        _runner = FindObjectOfType<NetworkRunner>();
        if (_runner == null)
        {
            _runner = Instantiate(_networkRunnerPrefab);
        }
        _runner.ProvideInput = true;


        // ovo se ne koristi trenutno
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex + 1);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }
        //


        await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = code,
            //Scene = scene,
        });

        if (_runner.IsServer)
        {
            await _runner.LoadScene(sceneName);
        }
    }


    // ako je host kod se nasumicno generira
    private string GenerateRoomCode()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[4];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

}
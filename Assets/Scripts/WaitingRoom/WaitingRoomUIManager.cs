using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;
using TMPro;


// ovo je spojeno na UI canvas i mijenja elemente na osnovu info iz waiting room managera
public class WaitingRoomUIManager : MonoBehaviour
{
    private static WaitingRoomUIManager _instance;
    public static WaitingRoomUIManager Instance => _instance;

    [Header("Waiting Room Elements")]
    [SerializeField] private TMP_Text  roomCodeText;
    [SerializeField] private TMP_Text  roomCode;
    [SerializeField] private TMP_Text  playersJoined;
    [SerializeField] private TMP_Text  timerText;
    [SerializeField] private TMP_Text  playersReady;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button customizeButton;
    [SerializeField] private Button settingsButton;

    [Header("Customization")]
    [SerializeField] private GameObject customizationPanel;


    private void Start()
    {
        _instance = this;

        readyButton.onClick.AddListener(OnReadyClicked);
        customizeButton.onClick.AddListener(OpenCustomization);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    private void OnReadyClicked()
    {
        if (!WaitingRoomManager.Instance)
            return;
        WaitingRoomManager.Instance.SetPlayerReady();
    }

    private void OpenCustomization()
    {
        customizationPanel.SetActive(true);
    }

    private void OpenSettings()
    {
        // todo
    }

    public void UpdateTimer(int timeRemaining)
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    public void UpdatePlayersJoined(int totalCount, int maxCount)
    {
        playersJoined.text = totalCount.ToString() + "/" + maxCount.ToString();
    }

    public void UpdatePlayersReady(int readyCount, int totalCount)
    {
        playersReady.text = readyCount.ToString() + "/" + totalCount.ToString();
    }

    public void SetRoomCode(string code)
    {
        roomCode.text = code;
        Debug.Log(code);
    }
}

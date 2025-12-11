using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class WaitingRoomUIManager : MonoBehaviour
{
    private static WaitingRoomManager _instance;
    public static WaitingRoomManager Instance => _instance;

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
    public List<Button> colorButtons;


    private void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
        customizeButton.onClick.AddListener(OpenCustomization);
        settingsButton.onClick.AddListener(OpenSettings);


        for (int i = 0; i < colorButtons.Count; i++)
        {
            int index = i; 
            colorButtons[i].onClick.AddListener(() => ChangeColor(index));
        }

    }

    private void OnReadyClicked()
    {
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

    private void ChangeColor(int index)
    {
        WaitingRoomManager.Instance.ChangePlayerColor(index);
    }

    public void UpdateTimer(int timeRemaining)
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }
    public void UpdatePlayersJoined(int totalCount)
    {
        playersJoined.text = totalCount.ToString() + "/6";
    }

    public void UpdatePlayersReady(int readyCount, int totalCount)
    {
        playersReady.text = readyCount.ToString() + "/" + totalCount.ToString();
    }


    public void SetRoomCode(string code)
    {
        roomCodeText.text = code;
    }
}

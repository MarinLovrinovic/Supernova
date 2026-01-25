using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Fusion;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TMP_Text livesText;

    private BattleManager battleManager;
    private bool hasReturnedToMenu = false;

    void Start()
    {
        battleManager = BattleManager.Instance;

        if (battleManager != null && battleManager.Runner != null)
        {
            bool iAmWinner = battleManager.FinalWinner == battleManager.Runner.LocalPlayer;

            int myFinalLives = 0;
            if (battleManager.Runner.TryGetPlayerObject(battleManager.Runner.LocalPlayer, out var myPlayerObj))
            {
                var myPlayerData = myPlayerObj.GetComponent<PlayerNetworkData>();
                if (myPlayerData != null)
                {
                    myFinalLives = myPlayerData.Lives;
                }
            }

            if (iAmWinner)
            {
                if (winnerText != null)
                    winnerText.text = "VICTORY!";
                if (messageText != null)
                    messageText.text = "You are the champion!";
            }
            else
            {
                if (winnerText != null)
                    winnerText.text = "GAME OVER";
                if (messageText != null)
                {
                    string winnerName = "Another player";
                    if (battleManager.Runner.TryGetPlayerObject(battleManager.FinalWinner, out var winnerObj))
                    {
                        var winnerData = winnerObj.GetComponent<PlayerNetworkData>();
                        if (winnerData != null)
                        {
                            winnerName = $"Player {battleManager.FinalWinner}";
                        }
                    }
                    messageText.text = $"{winnerName} won!";
                }
            }

            if (livesText != null)
            {
                livesText.text = $"Final Lives: {myFinalLives}";
            }
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    void Update()
    {
        if (!hasReturnedToMenu)
        {
            bool isDisconnected = false;

            if (battleManager == null)
            {
                isDisconnected = true;
            }
            else if (battleManager.Runner == null)
            {
                isDisconnected = true;
            }
            else if (!battleManager.Runner.IsRunning)
            {
                isDisconnected = true;
            }
            else if (battleManager.Object != null && !battleManager.Object.IsValid)
            {
                isDisconnected = true;
            }

            if (isDisconnected)
            {
                Debug.Log("[GameOver] Detected disconnection, returning to menu");
                hasReturnedToMenu = true;
                StartCoroutine(ReturnToMenuAfterDelay());
            }
        }
    }

    void OnMainMenuClicked()
    {
        if (hasReturnedToMenu) return;

        hasReturnedToMenu = true;

        if (mainMenuButton != null)
            mainMenuButton.interactable = false;

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);

        try
        {
            PlayerNetworkData[] allPlayerData = FindObjectsOfType<PlayerNetworkData>(true);
            foreach (var playerData in allPlayerData)
            {
                if (playerData != null)
                {
                    Destroy(playerData.gameObject);
                }
            }
            PlayerNetworkData.Local = null;

            if (battleManager != null && battleManager.Runner != null && battleManager.Runner.IsRunning)
            {
                battleManager.Runner.Shutdown();
            }

            if (BattleManager.Instance != null)
            {
                Destroy(BattleManager.Instance.gameObject);
                BattleManager.Instance = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GameOver] Error during cleanup: {e.Message}");
        }

        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
    }
}
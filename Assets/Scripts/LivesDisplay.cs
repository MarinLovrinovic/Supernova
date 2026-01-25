using UnityEngine;
using TMPro;
using Fusion;

public class LivesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject livesContainer;

    private PlayerNetworkData localPlayerData;
    private bool isValid = false;

    void Start()
    {
        UpdatePlayerReference();
    }

    void Update()
    {
        if (localPlayerData == null)
        {
            UpdatePlayerReference();
        }

        if (localPlayerData != null)
        {
            if (localPlayerData.Object == null || !localPlayerData.Object.IsValid)
            {
                localPlayerData = null;
                isValid = false;
                if (livesContainer != null)
                    livesContainer.SetActive(false);
                return;
            }
        }

        if (localPlayerData != null && livesText != null)
        {
            try
            {
                int lives = localPlayerData.Lives;
                livesText.text = $"Lives: {lives}";

                if (livesContainer != null)
                {
                    livesContainer.SetActive(lives > 0);
                }

                isValid = true;
            }
            catch (System.InvalidOperationException)
            {
                localPlayerData = null;
                isValid = false;
                if (livesContainer != null)
                    livesContainer.SetActive(false);
            }
        }
        else if (livesContainer != null)
        {
            livesContainer.SetActive(false);
        }
    }

    private void UpdatePlayerReference()
    {
        if (PlayerNetworkData.Local != null)
        {
            if (PlayerNetworkData.Local.Object != null && PlayerNetworkData.Local.Object.IsValid)
            {
                localPlayerData = PlayerNetworkData.Local;
                isValid = true;
            }
        }
        else
        {
            if (BattleManager.Instance != null && BattleManager.Instance.Runner != null)
            {
                if (BattleManager.Instance.Runner.TryGetPlayerObject(
                    BattleManager.Instance.Runner.LocalPlayer, out var playerObj))
                {
                    var playerData = playerObj.GetComponent<PlayerNetworkData>();
                    if (playerData != null && playerObj.IsValid)
                    {
                        localPlayerData = playerData;
                        isValid = true;
                    }
                }
            }
        }
    }
}
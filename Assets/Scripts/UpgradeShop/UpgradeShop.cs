using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using Fusion; 

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] private Transform upgradeButtonsContainer;
    [SerializeField] private Button acceptButton;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Sprite[] UpgradeSprites;
    [SerializeField] private int numberOfUpgradesDisplayed;
    
    [SerializeField] private GameObject upgradeShopEmpty; 
    [SerializeField] private GameObject afterAcceptEmpty;
    [SerializeField] private GameObject winnerViewEmpty;
    
    [SerializeField] private TMP_Text winnerCounter;
    [SerializeField] private TMP_Text afterAcceptCounter;
    [SerializeField] private Image afterAcceptUpgradeImage;
    
    
    private Upgrades chosenUpgrade;                          
    public bool upgradeIsSelected { get; private set; } = false; 
    
    private Button[] buttons;
    
    
    void Start()
    {
        Debug.Log("Upgrade Shop");

        if (battleManager == null)
            battleManager = BattleManager.Instance;

        bool iAmWinner = battleManager != null && battleManager.WinnerRef != default && 
                         battleManager.Runner != null && battleManager.Runner.LocalPlayer == battleManager.WinnerRef;
        
        if (upgradeShopEmpty != null) upgradeShopEmpty.SetActive(false); 
        if (afterAcceptEmpty != null) afterAcceptEmpty.SetActive(false); 
        if (winnerViewEmpty != null) winnerViewEmpty.SetActive(false);   
        
        if (iAmWinner)
        {
            if (winnerViewEmpty != null) winnerViewEmpty.SetActive(true); 
        }
        else
        {
            if (upgradeShopEmpty != null) upgradeShopEmpty.SetActive(true); 
        }
        
        Debug.Log($"[UpgradeShop Views] shop={(upgradeShopEmpty != null && upgradeShopEmpty.activeSelf)} " +
                  $"after={(afterAcceptEmpty != null && afterAcceptEmpty.activeSelf)} " +
                  $"winner={(winnerViewEmpty != null && winnerViewEmpty.activeSelf)}"); 

        buttons = upgradeButtonsContainer.GetComponentsInChildren<Button>();
        SpawnButtons(numberOfUpgradesDisplayed);
        
    }
    
    void Update() 
    {
        bool afterAcceptActive = (afterAcceptEmpty != null && afterAcceptEmpty.activeSelf); 
        bool winnerViewActive = (winnerViewEmpty != null && winnerViewEmpty.activeSelf);   
        
        if (!afterAcceptActive && !winnerViewActive) return;                
        
        string text;
        
        if (afterAcceptCounter == null) return;
        if (BattleManager.Instance == null) return;

        if (BattleManager.Instance.NextRoundCountdownRunning)
        {
            float t = BattleManager.Instance.GetNextRoundRemaining();
            int sec = Mathf.CeilToInt(t);
            text = sec.ToString();
        }
        else
        {
            text = BattleManager.Instance.AcceptedCount + " / " + BattleManager.Instance.AcceptRequired;
            
        }
        
        if (afterAcceptActive && afterAcceptCounter != null)
            afterAcceptCounter.text = text;

        if (winnerViewActive && winnerCounter != null)
            winnerCounter.text = text;
    }
    
    T[] GetRandomEnumValues<T>(int count)
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return values.OrderBy(x => UnityEngine.Random.value).Take(count).ToArray();
    }
    
    void SpawnButtons(int count)
    {
        
        Debug.Log("Spawn Buttons");
        var randomUpgrades = GetRandomEnumValues<Upgrades>(count);
        int i = 0;
        Debug.Log("Random upgrades size: " + randomUpgrades.Length);
        Debug.Log("Button in container: " + buttons.Length);
  
        for(int j = 0; j < randomUpgrades.Length; j++)
        {
            if (buttons.Length == randomUpgrades.Length)
            {
                Debug.Log("upgrade loop: " + randomUpgrades[j].ToString());
                
                var img = buttons[j].GetComponent<Image>();

                if (img != null)
                {
                    int idx = (int)(object)randomUpgrades[j];

                    if (UpgradeSprites != null && idx >= 0 && idx < UpgradeSprites.Length)
                        img.sprite = UpgradeSprites[idx];
                    img.preserveAspect = true;
                }

                var upgradeButtonText = buttons[j].GetComponentInChildren<TMP_Text>();
                if (upgradeButtonText != null) upgradeButtonText.text = "";


                var captured = randomUpgrades[j];
                buttons[j].onClick.RemoveAllListeners();

                buttons[j].onClick.AddListener(() =>
                {
                    Debug.Log($"Clicked {captured}");
                    chosenUpgrade = captured;
                    upgradeIsSelected = true;
                                 
                });
                
                i++;
            }
        }
        
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() =>
        {
            Debug.Log($"Clicked Accept");
            if (!upgradeIsSelected) return;

            acceptButton.interactable = false;
            
            if (upgradeShopEmpty != null) upgradeShopEmpty.SetActive(false);
            if (afterAcceptEmpty != null) afterAcceptEmpty.SetActive(true);

            if (afterAcceptUpgradeImage != null && UpgradeSprites != null)
            {
                int idx = (int)(object)chosenUpgrade;
                if (idx >= 0 && idx < UpgradeSprites.Length)
                {
                    afterAcceptUpgradeImage.sprite = UpgradeSprites[idx];
                    afterAcceptUpgradeImage.preserveAspect = true;
                }
            }
            
            if (battleManager != null && battleManager.Runner != null)
            {
                battleManager.RPC_SubmitUpgrade(battleManager.Runner.LocalPlayer, chosenUpgrade);
                
                if (PlayerNetworkData.Local != null)
                    PlayerNetworkData.Local.RPC_ClickedAccept();

                if (PlayerNetworkData.Local != null)
                {
                    var health = PlayerNetworkData.Local.GetComponent<Health>();
                    if (health != null) health.ResumeAfterShopLocal();
                }
            }
                          
        });
        
        var contRt = upgradeButtonsContainer as RectTransform;
        if (contRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contRt);
    }
    
}

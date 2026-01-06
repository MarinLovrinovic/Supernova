using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] private Transform upgradeButtonsContainer;
    [SerializeField] private Button acceptButton;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Sprite[] UpgradeSprites;
    [SerializeField] private int numberOfUpgradesDisplayed;
    
    
    private Upgrades chosenUpgrade;                          
    public bool upgradeIsSelected { get; private set; } = false; 
    public bool accept { get; private set; } = false;
    public Upgrades GetLastSelectedUpgrade() => chosenUpgrade;
    public event Action<Upgrades> UpgradeChosen;
    private Button[] buttons;
    
    
    void Start()
    {
        Debug.Log("Upgrade Shop");
        buttons = upgradeButtonsContainer.GetComponentsInChildren<Button>();
        
        SpawnButtons(numberOfUpgradesDisplayed);
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
        
        acceptButton.onClick.AddListener(() =>
        {
            Debug.Log($"Clicked Accept");
                
             
            if (battleManager != null && upgradeIsSelected)                   
            {                       
                Debug.Log($"battleManager != null");
                battleManager.StoreUpgradeAndCloseShop(chosenUpgrade); 
            }              
        });
        
        
        var contRt = upgradeButtonsContainer as RectTransform;
        if (contRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contRt);
    }
    
    
    
    
    
    
}

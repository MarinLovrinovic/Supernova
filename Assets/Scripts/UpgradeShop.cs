using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Transform Panel;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button acceptButtonPrefab;
    [SerializeField] private Sprite[] UpgradeSprites;
    [SerializeField] private int numberOfUpgradesDisplayed;
    
    private Upgrades chosenUpgrade;                          
    public bool upgradeIsSelected { get; private set; } = false; 
    public bool accept { get; private set; } = false;
    public Upgrades GetLastSelectedUpgrade() => chosenUpgrade;
    public event Action<Upgrades> UpgradeChosen; 
    
    void Start()
    {
        Debug.Log("Upgrade Shop");
        SpawnButtons(numberOfUpgradesDisplayed);
            
    }
    
    T[] GetRandomEnumValues<T>(int count)
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return values.OrderBy(x => UnityEngine.Random.value).Take(count).ToArray();
    }
    
    void SpawnButtons(int count)
    {
        
        // for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        // {
        //     GameObject.Destroy(buttonContainer.GetChild(i).gameObject);
        // }
        Debug.Log("Spawn Buttons");
        var randomUpgrades = GetRandomEnumValues<Upgrades>(count);
        int i = 0;

        var acceptButton = Instantiate(acceptButtonPrefab);
        acceptButton.transform.SetParent(Panel, false);
        acceptButton.gameObject.SetActive(true);
        
        
        var acceptButtonText = acceptButton.GetComponentInChildren<TMP_Text>();
        if (acceptButtonText != null) acceptButtonText.text = "Accept";
        
        
        
            
        foreach (var upgrade in randomUpgrades)
        {
            Debug.Log("upgrade loop: " + upgrade.ToString());
            
            var upgradeButton = Instantiate(buttonPrefab);
            upgradeButton.transform.SetParent(buttonContainer, false);
            upgradeButton.gameObject.SetActive(true);
            
            var rt = upgradeButton.GetComponent<RectTransform>();
            
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                if (rt.sizeDelta == Vector2.zero)
                {
                    rt.sizeDelta = new Vector2(160f, 60f);
                }    
                             
            }
            
            var img = upgradeButton.GetComponent<Image>();
            
            if (img != null)
            {
                int idx = (int)(object)upgrade;
                
                if (UpgradeSprites != null && idx >= 0 && idx < UpgradeSprites.Length)
                    img.sprite = UpgradeSprites[idx];
                img.preserveAspect = true;
            }
            
            var upgradeButtonText = upgradeButton.GetComponentInChildren<TMP_Text>();
            if (upgradeButtonText != null) upgradeButtonText.text = "";
            
            
            var captured = upgrade;
            upgradeButton.onClick.RemoveAllListeners();
            
            upgradeButton.onClick.AddListener(() =>
            {
                Debug.Log($"Clicked {captured}");
                chosenUpgrade = captured;        
                upgradeIsSelected = true;            
                // UpgradeChosen?.Invoke(captured);
                
                // var gameController = GameController.Instance; 
                // if (gameController != null)                   
                // {                                           
                //     gameController.StoreUpgradeAndCloseShop(upgrade); 
                // }              
            });
            i++;
        }
        
        acceptButton.onClick.AddListener(() =>
        {
            Debug.Log($"Clicked Accept");
                
            var gameController = GameController.Instance; 
            if (gameController != null && upgradeIsSelected)                   
            {                                           
                gameController.StoreUpgradeAndCloseShop(chosenUpgrade); 
            }              
        });
        
        
        var contRt = buttonContainer as RectTransform;
        if (contRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contRt);
    }
    
    
    
    
    
    
}

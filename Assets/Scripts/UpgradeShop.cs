using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Sprite[] UpgradeSprites;
    [SerializeField] private int numberOfUpgradesDisplayed;
    
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
        
        foreach (var upgrade in randomUpgrades)
        {
            Debug.Log("upgrade loop: " + upgrade.ToString());
            
            var button = Instantiate(buttonPrefab);
            button.transform.SetParent(buttonContainer, false);
            button.gameObject.SetActive(true);
            
            var rt = button.GetComponent<RectTransform>();
            
            if (rt != null)
            {
                Debug.Log("rt != null");
                rt.localScale = Vector3.one;
                if (rt.sizeDelta == Vector2.zero)
                {
                    rt.sizeDelta = new Vector2(160f, 60f);
                }    
                             
            }
            
            var img = button.GetComponent<Image>();
            
            if (img != null)
            {
                int idx = (int)(object)upgrade;
                Debug.Log("img != null: " + idx);
                
                if (UpgradeSprites != null && idx >= 0 && idx < UpgradeSprites.Length)
                    img.sprite = UpgradeSprites[idx];
                img.preserveAspect = true;
            }
            
            // var textBox = button.GetComponentInChildren<TMP_Text>();
            // if (textBox != null) textBox.text = upgrade.ToString();
            
            
            var captured = upgrade;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log($"Clicked {captured}");
                // do something with `captured`
            });
            
            i++;
        }
        
        var contRt = buttonContainer as RectTransform;
        if (contRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contRt);
    }
    
    
    
    
    
    
}

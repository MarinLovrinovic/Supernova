using TMPro;
using UnityEngine;

public class WinnerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counter;
    private BattleManager bm; 

    void Start()
    {
        bm = BattleManager.Instance;
        if (bm == null)
            bm = FindObjectOfType<BattleManager>(); 

        Debug.Log("WinnerUI bm = " + (bm != null ? bm.name : "NULL")); 
    }

    void Update()
    {
        if (counter == null) return;
        
        if (bm == null)
        {
            bm = BattleManager.Instance;
            if (bm == null) return;
        }

        if (bm.NextRoundCountdownRunning)
        {
            float t = bm.GetNextRoundRemaining();
            counter.text = Mathf.CeilToInt(t).ToString();
        }
        else
        {
            counter.text = bm.AcceptedCount + " / " + bm.AcceptRequired;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Customization : MonoBehaviour
{
    public static Customization Instance;
    public List<Button> colorButtons;
    //[SerializeField] public List<Color> palette;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < colorButtons.Count; i++)
        {
            int index = i;
            colorButtons[index].onClick.AddListener(() => OnColorPressed(index));
        }
        RefreshUI();
    }

    private void OnColorPressed(int index)
    {
        var local = WaitingRoomManager.Instance.LocalPlayer;

        if (local == null)
            return;

        if (!local.Object.HasInputAuthority)
            return;

        local.RPC_RequestColorChange(index);
    }


    // da se vidi koje su boje zauzete/odabrane
    public void RefreshUI()
    {
        if (!WaitingRoomManager.Instance)
            return;

        var wrm = WaitingRoomManager.Instance;
        var runner = wrm.Runner;
        var localPlayer = wrm.LocalPlayer;

        if (localPlayer == null)
            return;


        // reset
        for (int i = 0; i < colorButtons.Count; i++)
        {
            bool taken = false;
            bool mine = false;

            foreach (var p in runner.ActivePlayers)
            {
                if (!runner.TryGetPlayerObject(p, out var obj))
                    continue;

                var pnd = obj.GetComponent<PlayerNetworkData>();

                if (pnd.ColorIndex == i)
                {
                    taken = true;
                    if (pnd == localPlayer)
                        mine = true;
                }
            }

            // X ako je zauzeta od nekog drugog
            colorButtons[i].transform.Find("UnavailableX")
                ?.gameObject.SetActive(taken && !mine);

            // okvir ako je moja
            colorButtons[i].transform.Find("SelectedFrame")
                ?.gameObject.SetActive(mine);

            // klik
            colorButtons[i].interactable = !taken || mine;
        }
    }

}


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Customization : MonoBehaviour
{
    public static Customization Instance;
    public List<Button> colorButtons;
    [SerializeField] public List<Color> palette;

    
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

    // da se vidi koje su boje zauzete/odabrane
    public void RefreshUI()
    {
        var wrm = WaitingRoomManager.Instance;
        if (!wrm) return;

        var localPlayer = wrm.LocalPlayer;

        if (localPlayer == null)
            return;

        int localColor = localPlayer.ColorIndex;

        for (int i = 0; i < colorButtons.Count; i++)
        {
            bool taken = wrm.IsColorTaken(i);
            //Debug.Log("[Customization.RefreshUI] Color " + i + " taken: " + taken);
            //Debug.Log("[Customization.RefreshUI] Local player color: " + localColor);


            // prikaz X ako je boja zauzeta od nekog drugog
            Transform xMark = colorButtons[i].transform.Find("UnavailableX");
            xMark?.gameObject.SetActive(taken && localColor != i);


            // prikaz okvira ako je moja trenutna boja
            Transform frame = colorButtons[i].transform.Find("SelectedFrame");
            frame?.gameObject.SetActive(localColor == i);


            // onemoguci klik ako je boja zauzeta ili je to moja boja
            Button btn = colorButtons[i];
            btn.interactable = !taken || localColor == i;
        }
    }


    private void OnColorPressed(int index)
    {
        Debug.Log("Color changed to: " + palette[index].ToString());
        WaitingRoomManager.Instance.ChangePlayerColor(index);
    }


    public static Color getColor(int index)
    {
        if (Instance == null || index < 0 || index >= Instance.palette.Count)
            return Color.white;

        return Instance.palette[index];
    }

}


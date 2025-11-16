using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public int playerNumber;
    public Transform playerContainer;
    public GameObject defaultPlayerPrefab;
    public GameObject defaultWeaponPrefab;
    public GameObject defaultShieldPrefab;
    public GameObject upgradeShopPrefab;

    private List<GameObject> livePlayers;
    private List<PlayerData> playerData;
    private bool battlePhaseActive;
    private Upgrades chosenUpgrade;

    private Vector3 topRightPos;
    private Vector3 bottomLeftPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Camera camera = Camera.main;
        topRightPos = camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
        bottomLeftPos = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f));
    }

    void Start()
    {
        livePlayers = new List<GameObject>();
        playerData = new List<PlayerData>();

        battlePhaseActive = false;
        SpawnPlayers();
        AsteroidSpawner.Instance.SpawnAsteroids();
        battlePhaseActive = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))//samo za testiranje NE DIRAJ ZA SAD
        {
            ShopEnd();
        }
        
        
        if (battlePhaseActive)
        {
            if (livePlayers.Count == 1) RoundEnd();
        }
        else
        {
            // ShopEnd();  //TODO: Napravit logiku za shopping fazu, zasad samo odma zavrsi i krece nova battle faza
        }
    }

    public void PlayerDestroyed(GameObject destroyedPlayer)
    {
        if (livePlayers.Count > 1 && livePlayers.Contains(destroyedPlayer)) livePlayers.Remove(destroyedPlayer);
    }

    private void SpawnPlayers()
    {
        if (playerNumber < 1)
        {
            Debug.Log("Too few players!");
            return;
        }

        if (playerData.Count == 0)
        {
            for (int i = 0; i < playerNumber; i++)
            {
                PlayerData data = ScriptableObject.CreateInstance<PlayerData>();
                data.playerPrefab = defaultPlayerPrefab;
                data.weaponPrefab = defaultWeaponPrefab;
                data.shieldPrefab = defaultShieldPrefab;

                playerData.Add(data);
            }
        }

        foreach (var data in playerData)
        {
            Vector3 spawnPos = new Vector3(Random.Range(bottomLeftPos.x, topRightPos.x), Random.Range(bottomLeftPos.y, topRightPos.y), 0.0f);
            GameObject player = Instantiate(data.playerPrefab, spawnPos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
            player.transform.SetParent(playerContainer.transform, worldPositionStays: true);

            Instantiate(data.weaponPrefab, player.transform);
            Instantiate(data.shieldPrefab, player.transform);

            livePlayers.Add(player);
        }
    }

    private void RoundEnd()
    {
        battlePhaseActive = false;

        GameObject winner = livePlayers[0];
        Debug.Log("Winner: " + winner.name);

        Destroy(livePlayers[0]);
        livePlayers.Clear();

        AsteroidSpawner.Instance.DespawnAsteroids();
    }

    private void ShopEnd()
    {
        
        var existingShop = FindObjectOfType<UpgradeShop>();
        
        if (existingShop != null)
            Destroy(existingShop.gameObject);

        var clone = Instantiate(upgradeShopPrefab, Vector3.zero, Quaternion.identity);
        
        var canvas = clone.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 100;
            
        }
        
        Debug.Log("Shop end");
        SpawnPlayers();
        AsteroidSpawner.Instance.SpawnAsteroids();
        battlePhaseActive = true;
        
    }
    
    public void StoreUpgradeAndCloseShop(Upgrades selected)
    {
        chosenUpgrade = selected;
        Debug.Log("Stored last upgrade: " + chosenUpgrade);

        
        var shop = FindObjectOfType<UpgradeShop>();
        if (shop != null)
            Destroy(shop.gameObject);

        
        
    }
}

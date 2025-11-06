using System.Text;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private float fireRate;
    [SerializeField] private float fireTimer;

    private PlayerControls pc;
    private bool isShooting;

    private void Awake()
    {
        pc = new PlayerControls();

        pc.Gameplay.Shoot.performed += ctx => isShooting = true;
        pc.Gameplay.Shoot.canceled += ctx => isShooting = false;
    }

    private void OnEnable() => pc.Enable();
    private void OnDisable() => pc.Disable();

    void Update()
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        if (isShooting && fireTimer <= 0f)
        {
            ShootWeapon();
            fireTimer = fireRate;
        }
    }


    private void ShootWeapon()
    {
        if (projectilePrefab == null || firingPoint == null)
        {
            Debug.LogWarning("Projectile prefab or firing point not assigned!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firingPoint.position, firingPoint.rotation);

        Projectile script = projectile.GetComponent<Projectile>();
        if (script != null) script.owner = this.gameObject;
    }
}
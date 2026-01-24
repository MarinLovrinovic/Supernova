using UnityEngine;
using Fusion;

namespace DefaultNamespace
{
    public class PlayerWeapon : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef projectilePrefab;
        [SerializeField] private Transform firingPoint;
        [SerializeField] private float fireRate;

        [Networked] private NetworkButtons _buttonsPrevious { get; set; }
        [Networked] private TickTimer _shootCooldown { get; set; }
        [Networked] public WeaponType CurrentWeapon { get; set; }

        private Transform rbWeapon;
        public override void Spawned()
        {
            rbWeapon = GetComponent<Transform>();
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.TryGetInputForPlayer<NetworkInputData>(Object.InputAuthority, out var input))
            {
                ShootWeapon(input);
            }
        }

        private void ShootWeapon(NetworkInputData input)
        {
            if (input.Buttons.WasPressed(_buttonsPrevious, SpaceshipButtons.Fire))
            {
                SpawnProjectile();
            }
        }
        private void SpawnProjectile()
        {
            if (_shootCooldown.ExpiredOrNotRunning(Runner) == false || !Runner.CanSpawn) return;

            if (!projectilePrefab.IsValid)
            {
                Debug.LogError("Projectile prefab is not set or not registered in NetworkProjectConfig!");
                return;
            }

            NetworkObject proj = Runner.Spawn(projectilePrefab, rbWeapon.position, rbWeapon.transform.rotation, Object.InputAuthority);

            if (proj.TryGetComponent(out Projectile projectile))
            {
                projectile.projectileOwner = Object.InputAuthority;
            }

            _shootCooldown = TickTimer.CreateFromSeconds(Runner, fireRate);
        }

    }
    
}
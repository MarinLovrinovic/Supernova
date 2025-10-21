using UnityEditor.Search;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firingPoint;
        [SerializeField] private float fireRate;
        [SerializeField] private float fireTimer;
        
        private Rigidbody2D weapon;
        // Start is called before the first frame update
        void Start()
        {
            weapon = GetComponent<Rigidbody2D>();
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && fireTimer <= 0) //OVO JE AKO SVAKI KLIK JEDAN METAK
            //if (Input.GetKey(KeyCode.Space) && fireTimer <= 0) OVO JE AKO OCEMO DA PUCA CONTINUINIRANO
            {
                ShootWeapon();
                fireTimer = fireRate;
            }
            else
            {
                fireTimer -= Time.deltaTime;
            }
        }


        private void ShootWeapon()
        {
            Instantiate(projectilePrefab, firingPoint.position, firingPoint.rotation);
        }
        

        
    }
    
}
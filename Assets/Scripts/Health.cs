using UnityEngine;
using System.Collections;
using Fusion;
using Fusion.Addons.Physics;

public class Health : NetworkBehaviour {

    public enum deathAction { doNothingWhenDead, destroyedWhenDead };

    public float healthPoints = 20f;
    public float respawnHealthPoints = 20f;
    public int numberOfLives = 1;
    public bool isAlive = true;
    public deathAction onLivesGone = deathAction.doNothingWhenDead;
    private Rigidbody2D rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (healthPoints <= 0)
        {
            numberOfLives--;

            if (numberOfLives > 0)
            {
                healthPoints = respawnHealthPoints;
            }
            else
            {
                isAlive = false;

                switch (onLivesGone)
                {
                    case deathAction.destroyedWhenDead:
                        Destroy(rb);
                        break;
                    case deathAction.doNothingWhenDead:
                        break;
                }
                Destroy(gameObject);
            }
        }
    }

    public void ApplyDamage(float amount)
    {
        healthPoints = healthPoints - amount;
    }
}
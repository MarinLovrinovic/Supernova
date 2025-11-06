using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
	
	public enum deathAction {doNothingWhenDead, destroyedWhenDead};

	public float healthPoints = 20f;
	public float respawnHealthPoints = 20f;
	public int numberOfLives = 1;
	public bool isAlive = true;
	public deathAction onLivesGone = deathAction.doNothingWhenDead;
	private Rigidbody2D rb;

	void Start () 
	{
		rb = GetComponent<Rigidbody2D>();
	}
	
	void Update () 
	{
		if (healthPoints <= 0) {
			numberOfLives--;
			
			if (numberOfLives > 0) {
				healthPoints = respawnHealthPoints;
			} else {
				isAlive = false;

				if (gameObject.CompareTag("Player")) GameController.Instance.PlayerDestroyed(gameObject);
				
				switch(onLivesGone)
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

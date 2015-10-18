using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable 
{
	public float startingHealth;
	protected float health;
	protected bool isDead;
	
	public event System.Action OnDeath;
	
	protected virtual void Start()
	{
		health = startingHealth;
	}
	
	public void TakeHit(float damage, RaycastHit hit)
	{
		health -= damage;
		
		if (health <= 0 && !isDead)
		{
			Die();
		}
	}
	
	protected void Die()
	{
		isDead = true;
		if (OnDeath != null) OnDeath();
		GameObject.Destroy(gameObject);
	}
}


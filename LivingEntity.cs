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
	
	public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		// Do things with hit
		TakeDamage(damage);
	}
	
	public virtual void TakeDamage(float damage)
	{
		health -= damage;
		
		if (health <= 0 && !isDead)
		{
			Die();
		}
	}
	
	[ContextMenu("Self Destruct")]
	protected void Die()
	{
		isDead = true;
		if (OnDeath != null) OnDeath();
		GameObject.Destroy(gameObject);
	}
}


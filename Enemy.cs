using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity 
{
	public enum State { Idle, Chasing, Attacking }
	State currentState;
	
	public ParticleSystem deathEffect;
	
	NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;
	Material skinMaterial;
	Color originalColor;
	
	float attackDistanceThreshold = 0.5f;
	float timeBetweenAttacks = 1;
	float damage = 1;
	
	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;
	
	bool hasTarget;
	
	void Awake()
	{
		pathfinder = GetComponent<NavMeshAgent>();
		
		if (GameObject.FindGameObjectWithTag("Player") != null)
		{
			hasTarget = true;
			
			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetEntity = target.GetComponent<LivingEntity>();
			
			myCollisionRadius = GetComponent<CapsuleCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
		}
	}
	
	protected override void Start() 
	{
		base.Start();
		
		if (hasTarget)
		{
			currentState = State.Chasing;
			targetEntity.OnDeath += OnTargetDeath;
			
			StartCoroutine(UpdatePath());	
		}
	}
	
	public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
	{
		pathfinder.speed = moveSpeed;
		
		if (hasTarget)
		{ 
			damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
		}
		
		startingHealth = enemyHealth;
		
		skinMaterial = GetComponent<Renderer>().material;
		skinMaterial.color = skinColor;
		originalColor = skinMaterial.color;
	}
	
	public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		if (damage >= health)
		{
			Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
		}
		
		base.TakeHit(damage, hitPoint, hitDirection);
	}
	
	void OnTargetDeath()
	{
		hasTarget = false;
		currentState = State.Idle;
	}
	
	void Update () 
	{
		if (hasTarget)
		{
			if (Time.time > nextAttackTime)
			{
				var sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDistToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
				{
					nextAttackTime = Time.time + timeBetweenAttacks;
					StartCoroutine(Attack());
				}
			}	
		}
	}
	
	IEnumerator Attack()
	{
		currentState = State.Attacking;
		pathfinder.enabled = false;	
		
		var originalPosition = transform.position;
		var dirToTarget = (target.position - transform.position).normalized;
		var attackPosition = target.position - dirToTarget * (myCollisionRadius);
		
		var attackSpeed = 3f;
		var percent = 0f;
		
		skinMaterial.color = Color.red;
		bool hasAppliedDamage = false;
		
		while (percent <= 1)
		{
			if (percent >= 0.5f && !hasAppliedDamage)
			{
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}
			
			percent += Time.deltaTime * attackSpeed;
			var interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}	
		
		skinMaterial.color = originalColor;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}
	
	IEnumerator UpdatePath()
	{
		float refreshRate = 0.25f;
		
		while (hasTarget)
		{
			if (currentState == State.Chasing)
			{
				var dirToTarget = (target.position - transform.position).normalized;
				var targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!isDead) pathfinder.SetDestination(targetPosition);
			}
			
			yield return new WaitForSeconds(refreshRate);
		}
	}
}

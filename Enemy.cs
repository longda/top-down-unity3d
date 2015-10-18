using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity 
{
	public enum State { Idle, Chasing, Attacking }
	State currentState;
	
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
	
	protected override void Start () 
	{
		base.Start();
		pathfinder = GetComponent<NavMeshAgent>();
		skinMaterial = GetComponent<Renderer>().material;
		originalColor = skinMaterial.color; 
		
		if (GameObject.FindGameObjectWithTag("Player") != null)
		{
			currentState = State.Chasing;
			hasTarget = true;
			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetEntity = target.GetComponent<LivingEntity>();
			targetEntity.OnDeath += OnTargetDeath;
			myCollisionRadius = GetComponent<CapsuleCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
			
			StartCoroutine(UpdatePath());	
		}
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

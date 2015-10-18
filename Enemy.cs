using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity 
{
	NavMeshAgent pathfinder;
	Transform target;
	
	protected override void Start () 
	{
		base.Start();
		pathfinder = GetComponent<NavMeshAgent>();
		target = GameObject.FindGameObjectWithTag("Player").transform;
		
		StartCoroutine(UpdatePath());
	}
	
	void Update () 
	{
		
	}
	
	IEnumerator UpdatePath()
	{
		float refreshRate = 0.25f;
		
		while (target != null)
		{
			var targetPosition = new Vector3(target.position.x, 0, target.position.z);
			if (!isDead) pathfinder.SetDestination(targetPosition);
			yield return new WaitForSeconds(refreshRate);
		}
	}
}

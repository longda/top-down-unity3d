using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	public LayerMask collisionMask;
	float speed = 10;
	float damage = 1;
	
	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}
	
	void Update () 
	{
		float moveDistance = speed * Time.deltaTime;
		CheckCollisions(moveDistance);
		transform.Translate(Vector3.forward * Time.deltaTime * speed);
	}
	
	void CheckCollisions(float moveDistance)
	{
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide))
		{
			OnHitObject(hit);
		}
	}
	
	void OnHitObject(RaycastHit hit)
	{
		//print(hit.collider.gameObject.name);
		var damageableObject = hit.collider.GetComponent<IDamageable>();
		if (damageableObject != null) damageableObject.TakeHit(damage, hit);
		GameObject.Destroy(gameObject);
	}
}

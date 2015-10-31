﻿using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	public LayerMask collisionMask;
	float speed = 10;
	float damage = 1;
	
	float lifetime = 3;
	float skinWidth = 0.1f;
	
	void Start()
	{
		Destroy(gameObject, lifetime);
		
		var initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
		if (initialCollisions.Length > 0) OnHitObject(initialCollisions[0], transform.position);
	}
	
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
		
		if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
		{
			OnHitObject(hit.collider, hit.point);
		}
	}
	
	void OnHitObject(Collider c, Vector3 hitPoint)
	{
		var damageableObject = c.GetComponent<IDamageable>();
		if (damageableObject != null) damageableObject.TakeHit(damage, hitPoint, transform.forward);
		GameObject.Destroy(gameObject);
	}
}

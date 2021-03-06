﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity 
{
	public float moveSpeed = 5;
	
	public Crosshairs crosshairs;
	
	Camera viewCamera;
	PlayerController controller;
	GunController gunController;

	protected override void Start () 
	{
		base.Start();
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		viewCamera = Camera.main;
	}
	
	void Update () 
	{
		// Movement input
		var moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		var moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move(moveVelocity);
		
		// Look input
		var ray = viewCamera.ScreenPointToRay(Input.mousePosition);
		var groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
		float rayDistance;
		
		if (groundPlane.Raycast(ray, out rayDistance))
		{
			var point = ray.GetPoint(rayDistance);
			//Debug.DrawLine(ray.origin, point, Color.red);
			controller.LookAt(point);
			crosshairs.transform.position = point;
			crosshairs.DetectTargets(ray);
			
			if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
			{
				gunController.Aim(point);	
			}
		}
		
		// Weapon input
		if (Input.GetMouseButton(0))
		{
			gunController.OnTriggerHold();
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			gunController.OnTriggerRelease();
		}
		
		if (Input.GetKeyDown(KeyCode.R))
		{
			gunController.Reload();
		}
	}
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MuzzleFlash))]
public class Gun : MonoBehaviour 
{
	public Transform muzzle;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35;
	
	public Transform shell;
	public Transform shellEjector;
	MuzzleFlash muzzleFlash;
	
	void Start()
	{
		muzzleFlash = GetComponent<MuzzleFlash>();
	}
	
	float nextShotTime;
	
	public void Shoot()
	{
		if (Time.time > nextShotTime)
		{
			nextShotTime = Time.time + msBetweenShots / 1000;
			var newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
			newProjectile.SetSpeed(muzzleVelocity);
			
			Instantiate(shell, shellEjector.position, shellEjector.rotation);
			muzzleFlash.Activate();
		}	
	}
}

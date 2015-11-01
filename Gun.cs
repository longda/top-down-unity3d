using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MuzzleFlash))]
public class Gun : MonoBehaviour 
{
	public enum FireMode { Auto, Burst, Single };
	public FireMode fireMode;
	
	public Transform[] projectileSpawn;
	public Projectile projectile;
	public float msBetweenShots = 100f;
	public float muzzleVelocity = 35f;
	public int burstCount;
	
	public Transform shell;
	public Transform shellEjector;
	MuzzleFlash muzzleFlash;
	
	bool triggerReleasedSinceLastShot;
	int shotsRemainingInBurst;
	
	void Start()
	{
		muzzleFlash = GetComponent<MuzzleFlash>();
		shotsRemainingInBurst = burstCount;
	}
	
	float nextShotTime;
	
	void Shoot()
	{
		if (Time.time > nextShotTime)
		{
			if (fireMode == FireMode.Burst)
			{
				if (shotsRemainingInBurst == 0) return;
				shotsRemainingInBurst--;
			}
			else if (fireMode == FireMode.Single)
			{
				if (!triggerReleasedSinceLastShot) return;
			}
			
			for (var i = 0; i < projectileSpawn.Length; i++)
			{
				nextShotTime = Time.time + msBetweenShots / 1000;
				var newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
				newProjectile.SetSpeed(muzzleVelocity);
			}
			
			Instantiate(shell, shellEjector.position, shellEjector.rotation);
			muzzleFlash.Activate();
		}	
	}
	
	public void OnTriggerHold()
	{
		Shoot();
		triggerReleasedSinceLastShot = false;
	}
	
	public void OnTriggerRelease()
	{
		triggerReleasedSinceLastShot = true;
		shotsRemainingInBurst = burstCount;
	}
}

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
	public int projectilsPerMag;
	public float reloadTime = 0.3f;
	
	[Header("Recoil")]
	public Vector2 kickMinMax = new Vector2(0.5f, 0.2f);
	public Vector2 recoilAngleMinMax = new Vector2(3f, 5f);
	public float recoilMoveSettleTime = 0.1f;
	public float recoilRotationSettleTime = 0.1f;
	
	[Header("Effect")]
	public Transform shell;
	public Transform shellEjector;
	MuzzleFlash muzzleFlash;
	float nextShotTime;
	
	bool triggerReleasedSinceLastShot;
	int shotsRemainingInBurst;
	int projectilesRemainingInMag;
	bool isReloading;
	
	Vector3 recoilSmoothDampVelocity;
	float recoilRotationSmoothDampVelocity;
	float recoilAngle;
	
	void Start()
	{
		muzzleFlash = GetComponent<MuzzleFlash>();
		shotsRemainingInBurst = burstCount;
		projectilesRemainingInMag = projectilsPerMag;
	}
	
	void LateUpdate()
	{
		// animate recoil
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, 0.1f);
		recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationSettleTime);
		transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
		
		if (!isReloading && projectilesRemainingInMag == 0)
		{
			Reload();
		}
	}
	
	void Shoot()
	{
		if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
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
				if (projectilesRemainingInMag == 0) break;
				projectilesRemainingInMag--;
				nextShotTime = Time.time + msBetweenShots / 1000;
				var newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
				newProjectile.SetSpeed(muzzleVelocity);
			}
			
			Instantiate(shell, shellEjector.position, shellEjector.rotation);
			muzzleFlash.Activate();
			transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
			recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
			recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
		}	
	}
	
	public void Reload()
	{
		if (!isReloading && projectilesRemainingInMag != projectilsPerMag)
		{
			StartCoroutine(AnimateReload());	
		}
	}
	
	IEnumerator AnimateReload()
	{
		isReloading = true;
		yield return new WaitForSeconds(0.2f);
		
		float reloadSpeed = 1 / reloadTime;
		float percent = 0;
		
		while (percent < 1)
		{
			percent += Time.deltaTime * reloadSpeed;
			var maxReloadAngle = 30;
			var interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			var reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
			Vector3 initialRot = transform.localEulerAngles;
			
			transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
			
			yield return null;
		}
		
		isReloading = false;
		projectilesRemainingInMag = projectilsPerMag;
	}
	
	public void Aim(Vector3 aimPoint)
	{
		if (!isReloading)
		{
			transform.LookAt(aimPoint);	
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

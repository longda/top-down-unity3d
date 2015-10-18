using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour 
{
	public Enemy enemy;
	public Wave[] waves;
	
	Wave currentWave;
	int currentWaveNumber;
	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;
	
	void Start()
	{
		NextWave();
	}
	
	void Update()
	{
		if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
		{
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
			Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
			spawnedEnemy.OnDeath += OnEnemyDeath;
		}
	}
	
	void OnEnemyDeath()
	{
		enemiesRemainingAlive--;
		
		if (enemiesRemainingAlive == 0)
		{
			NextWave();
		}
	}
	
	void NextWave()
	{
		currentWaveNumber++;
		print(string.Format("Current Wave: {0}", currentWaveNumber));
		
		if (currentWaveNumber - 1 < waves.Length)
		{
			currentWave = waves[currentWaveNumber - 1];
			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;
		}
	}
	
	[System.Serializable]
	public class Wave
	{
		public int enemyCount;
		public float timeBetweenSpawns;
	}
}

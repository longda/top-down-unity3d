using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour 
{
	public Enemy enemy;
	public Wave[] waves;
	
	LivingEntity playerEntity;
	Transform playerT;
	
	Wave currentWave;
	int currentWaveNumber;
	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;
	
	MapGenerator map;
	
	float timeBetweenCampingChecks = 2f;
	float campThresholdDistance = 1.5f;
	float nextCampCheckTime;
	Vector3 campPositionOld;
	bool isCamping;
	
	bool isDisabled;
	
	public event System.Action<int> OnNewWave;
	
	void Start()
	{
		playerEntity = FindObjectOfType<Player>();
		playerT = playerEntity.transform;
		
		nextCampCheckTime = timeBetweenCampingChecks + Time.time;
		campPositionOld = playerT.position;
		playerEntity.OnDeath += OnPlayerDeath;
		
		map = FindObjectOfType<MapGenerator>();
		NextWave();
	}
	
	void Update()
	{
		if (!isDisabled)
		{
			if (Time.time > nextCampCheckTime)
			{
				nextCampCheckTime = timeBetweenCampingChecks + Time.time;
				isCamping = Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance;
				campPositionOld = playerT.position;
			}
			
			if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
			{
				enemiesRemainingToSpawn--;
				nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
				
				StartCoroutine(SpawnEnemy());
			}
		}
	}
	
	IEnumerator SpawnEnemy()
	{
		var spawnDelay = 1f;
		var tileFlashSpeed = 4f;
		
		Transform spawnTile = map.GetRandomOpenTile();
		if (isCamping) spawnTile = map.GetTileFromPosition(playerT.position);
		var tileMat = spawnTile.GetComponent<Renderer>().material;
		var initialColor = tileMat.color;
		var flashColor = Color.red;
		var spawnTimer = 0f;
		
		while (spawnTimer < spawnDelay)
		{
			tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
			spawnTimer += Time.deltaTime;
			
			yield return null;
		}
		
		Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;
	}
	
	void OnPlayerDeath()
	{
		isDisabled = true;
	}
	
	void OnEnemyDeath()
	{
		enemiesRemainingAlive--;
		
		if (enemiesRemainingAlive == 0)
		{
			NextWave();
		}
	}
	
	void ResetPlayerPosition()
	{
		playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
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
			
			if (OnNewWave != null) OnNewWave(currentWaveNumber);
			ResetPlayerPosition();
		}
	}
	
	[System.Serializable]
	public class Wave
	{
		public int enemyCount;
		public float timeBetweenSpawns;
	}
}

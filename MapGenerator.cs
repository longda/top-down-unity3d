using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
	public Map[] maps;
	public int mapIndex;
	
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	public Vector2 maxMapSize;
	
	[Range(0,1)]
	public float outlinePercent;
	
	public float tileSize;
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	Queue<Coord> shuffledOpenTileCoords;
	Transform[,] tileMap;
	
	Map currentMap;
	
	public void Start()
	{
		GenerateMap();
	}
	
	public void GenerateMap()
	{
		currentMap = maps[mapIndex];
		tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
		var rand = new System.Random(currentMap.seed);
		GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x, 0.05f, currentMap.mapSize.y);
		
		// Generating coords
		allTileCoords = new List<Coord>();
		for (var x = 0; x < currentMap.mapSize.x; x++)
		{
			for (var y = 0; y < currentMap.mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
		
		shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));
		
		// Create map holder object
		var holderName = "Generated Map";
		if (transform.FindChild(holderName)) 
		{
			DestroyImmediate(transform.FindChild(holderName).gameObject);
		}
		
		var mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = transform;
		
		// Spawning tiles
		for (var x = 0; x < currentMap.mapSize.x; x++)
		{
			for (var y = 0; y < currentMap.mapSize.y; y++)
			{
				var tilePosition = CoordToPosition(x, y);
				var newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x, y] = newTile;
			}
		}
		
		// Spawning obstacles
		bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
		
		var obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		var currentObstacleCount = 0;
		var allOpenCoords = new List<Coord>(allTileCoords);
		
		for (var i = 0; i < obstacleCount; i++)
		{
			var randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;
			
			if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				var obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)rand.NextDouble());
				var obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
				
				var newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
				newObstacle.parent = mapHolder;
				newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
				
				var obstacleRenderer = newObstacle.GetComponent<Renderer>();
				var obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
				float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
				obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
				obstacleRenderer.sharedMaterial = obstacleMaterial; 
				
				allOpenCoords.Remove(randomCoord);
			}
			else
			{
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		
		shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));
		
		// Creating the navmesh mask
		var maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform; 
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		var maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform; 
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		var maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform; 
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
		
		var maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform; 
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
		
		navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
	}
	
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
	{
		var mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		var queue = new Queue<Coord>();
		queue.Enqueue(currentMap.mapCenter);
		mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;
		
		var accessibleTileCount = 1;
		
		while (queue.Count > 0)
		{
			var tile = queue.Dequeue();
			
			for (var x = -1; x <= 1; x++)
			{
				for (var y = -1; y <= 1; y++)
				{
					var neighborX = tile.x + x;
					var neighborY = tile.y + y;
					
					if (x == 0 || y == 0)
					{
						if (neighborX >= 0 && neighborX < obstacleMap.GetLength(0) && neighborY >= 0 && neighborY < obstacleMap.GetLength(1))
						{
							if (!mapFlags[neighborX, neighborY] && !obstacleMap[neighborX, neighborY])
							{
								mapFlags[neighborX, neighborY] = true;
								queue.Enqueue(new Coord(neighborX, neighborY));
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}
		
		var targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		
		return targetAccessibleTileCount == accessibleTileCount;
	}
	
	Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
	}
	
	public Transform GetTileFromPosition(Vector3 position)
	{
		var x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		var y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
		y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
		
		return tileMap[x, y];
	}
	
	public Coord GetRandomCoord()
	{
		var randomCoord = shuffledTileCoords.Dequeue();
		shuffledTileCoords.Enqueue(randomCoord);
		
		return randomCoord;
	}
	
	public Transform GetRandomOpenTile()
	{
		var randomCoord = shuffledOpenTileCoords.Dequeue();
		shuffledOpenTileCoords.Enqueue(randomCoord);
		
		return tileMap[randomCoord.x, randomCoord.y];
	}
	
	[System.Serializable]
	public struct Coord
	{
		public int x;
		public int y;
		
		public Coord(int _x, int _y)
		{
			x = _x;
			y = _y;
		}
		
		public static bool operator ==(Coord c1, Coord c2)
		{
			return c1.x == c2.x && c1.y == c2.y;
		}
		
		public static bool operator !=(Coord c1, Coord c2)
		{
			return !(c1 == c2);
		}
	}
	
	[System.Serializable]
	public class Map
	{
		public Coord mapSize;
		[Range(0, 1)]
		public float obstaclePercent;
		public int seed;
		public float minObstacleHeight;
		public float maxObstacleHeight;
		public Color foregroundColor;
		public Color backgroundColor;
		
		public Coord mapCenter
		{
			get 
			{
				return new Coord(mapSize.x / 2, mapSize.y / 2);
			}
		}
	}
}

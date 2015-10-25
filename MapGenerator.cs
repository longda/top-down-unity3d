using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Transform navmeshFloor;
	public Transform navmeshMaskPrefab;
	public Vector2 mapSize;
	public Vector2 maxMapSize;
	
	[Range(0,1)]
	public float outlinePercent;
	[Range(0,1)]
	public float obstaclePercent;
	
	public float tileSize;
	
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	
	public int seed = 10;
	Coord mapCenter;
	
	public void Start()
	{
		GenerateMap();
	}
	
	public void GenerateMap()
	{
		allTileCoords = new List<Coord>();
		for (var x = 0; x < mapSize.x; x++)
		{
			for (var y = 0; y < mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
		
		shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
		mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
		
		var holderName = "Generated Map";
		if (transform.FindChild(holderName)) 
		{
			DestroyImmediate(transform.FindChild(holderName).gameObject);
		}
		
		var mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = transform;
		
		for (var x = 0; x < mapSize.x; x++)
		{
			for (var y = 0; y < mapSize.y; y++)
			{
				var tilePosition = CoordToPosition(x, y);
				var newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
			}
		}
		
		bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];
		
		var obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
		var currentObstacleCount = 0;
		
		for (var i = 0; i < obstacleCount; i++)
		{
			var randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;
			
			if (randomCoord != mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				var obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
				var newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform;
				newObstacle.parent = mapHolder;
				newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
			}
			else
			{
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		
		var maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform; 
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;
		
		var maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform; 
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;
		
		var maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform; 
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;
		
		var maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform; 
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;
		
		navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
	}
	
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
	{
		var mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		var queue = new Queue<Coord>();
		queue.Enqueue(mapCenter);
		mapFlags[mapCenter.x, mapCenter.y] = true;
		
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
		
		var targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
		
		return targetAccessibleTileCount == accessibleTileCount;
	}
	
	Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
	}
	
	public Coord GetRandomCoord()
	{
		var randomCoord = shuffledTileCoords.Dequeue();
		shuffledTileCoords.Enqueue(randomCoord);
		
		return randomCoord;
	}
	
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
}

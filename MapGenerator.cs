using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Vector2 mapSize;
	
	[Range(0,1)]
	public float outlinePercent;
	
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	
	public int seed = 10;
	
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
				var tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
				var newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent);
				newTile.parent = mapHolder;
			}
		}
		
		var obstacleCount = 10;
		for (var i = 0; i < obstacleCount; i++)
		{
			var randomCoord = GetRandomCoord();
			var obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
			var newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform;
			newObstacle.parent = mapHolder;
		}
	}
	
	Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
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
	}
}

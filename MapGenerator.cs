using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{
	public Transform tilePrefab;
	public Vector2 mapSize;
	
	[Range(0,1)]
	public float outlinePercent;
	
	public void Start()
	{
		GenerateMap();
	}
	
	public void GenerateMap()
	{
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
	}
}

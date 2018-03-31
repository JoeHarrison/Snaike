using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Vector2 mapSize;

	List<Coord> tileCoords;

	void Start(){
		GenerateMap();
	}

	public void GenerateMap(){

		tileCoords = new List<Coord> ();
		for (int x = 0; x < mapSize.x; x ++) {
			for (int y = 0; y < mapSize.y; y ++) {
				tileCoords.Add(new Coord(x,y));
			}
		}

		string holderName = "GenerateMap";
		if(transform.Find(holderName)){
				DestroyImmediate(transform.Find(holderName).gameObject);
		}

		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		for(int x = 0; x<mapSize.x; x++){
			for(int y = 0; y<mapSize.y; y++){
				Vector3 tilePosition = CoordToPosition(x,y);
				Transform newTile = Instantiate(tilePrefab,tilePosition,Quaternion.Euler(Vector3.right*90)) as Transform;
				newTile.parent = mapHolder;
			}
		}

		for (int x = 0; x < mapSize.x; x ++) {
			for (int y = 0; y < mapSize.y; y ++) {
				if((x ==0 || y==0) || (x==mapSize.x - 1 || y==mapSize.y - 1)){
					Vector3 obstaclePosition = CoordToPosition(x,y);
					Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * .5f, Quaternion.identity) as Transform;
					newObstacle.parent = mapHolder;
				}
			}
		}
	}

	Vector3 CoordToPosition(int x, int y) {
		return new Vector3 (-mapSize.x / 2 + x, 0, -mapSize.y / 2 + y);
	}

	public struct Coord {
		public int x;
		public int y;

		public Coord(int _x, int _y) {
			x = _x;
			y = _y;
		}
	}
}

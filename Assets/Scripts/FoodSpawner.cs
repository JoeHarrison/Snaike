using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {
	public int top = 4;
	public int bottom = -4;
	public int right = 4;
	public int left = -4;

	public GameObject FoodPrefab;
	public GameObject food;
	private Vector2 map;

	public Color color;
	public float timer;
	public float maxTime = 10f;

	public GameObject Spawn(){


		int x = (int) Random.Range(left,right);
		int z = (int) Random.Range(bottom,top);

		food = Instantiate(FoodPrefab, new Vector3(x,0.5f,z), Quaternion.identity);

		color = new Color(Random.value, Random.value, Random.value, 1.0f );
		food.GetComponent<Renderer>().material.color = color;

		return food;
	}

	void OnTriggerEnter(Collider collider) {
				if(collider.name.StartsWith("Tail")){
					print("Triggered");
					Destroy(food);
					food = Spawn();
				}
    }

	// Use this for initialization
	void Start () {
		//timer = Time.time;
		Vector2 map = GameObject.Find("Map").GetComponent<MapGenerator>().mapSize;
		top = (int) map.y / 2 - 1;
		bottom = (int) -map.y / 2 + 1;
		right = (int) map.x / 2 - 1;
		left = (int) -map.x / 2 + 1;
		food = Spawn();
	}

	// Update is called once per frame
	void Update () {
		// if(Time.time - timer>maxTime && food!=null){
		// 	timer = Time.time;
		// 	Destroy(food);
		// 	food = Spawn();
		// }
		if(food==null){
			//timer = Time.time;
			food = Spawn();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoeFusionSnake : MonoBehaviour, ISnake {

	public List<GameObject> Snake = new List<GameObject>();
	public GameObject Tail;

	public float scaleFactor;
	public float scaleFactorFood;
	public int[] networkStructure;

	public List<float> biases;
	public List<float[,]> weights;
	private float[] a;

	int TicTime;
	public int TicTimeLimit;

	public Vector3 previousDir = Vector3.zero;
	public Vector3 dir = Vector3.zero;

	public Text scoreText;
	public int score;

	public bool alive;
	public bool collidedWithWall;
	public bool collidedWithTail;
	public bool collidedWithHead;
	public float startTime;
	public float timeLastEaten;
	public float distanceFromFood;
	public float killTime;

	public Color color;

	public List<GameObject> getSnake(){
		return Snake;
	}

	public bool isAlive(){
		return alive;
	}

	public bool isCollidedWithWall(){
		return collidedWithWall;
	}

	public float getTimeAlive(){
		return Time.time - startTime;
	}

	public float getDistanceFromFood(){
		return distanceFromFood;
	}

	public int getScore(){
		return score;
	}

	public void Initialize(List<float[,]> weights, List<float> biases, int[] networkStructure, Color color){
		this.weights = weights;
		this.biases = biases;
		this.networkStructure = networkStructure;
		this.color = color;
	}

	void Inputs(){
		Vector3 ownPosition = transform.position;
		Vector3 foodPosition = GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().food.transform.position;
		Vector3 relativeFoodPosition = (foodPosition - ownPosition)/scaleFactor;

		Debug.DrawRay(transform.position, (foodPosition - ownPosition), Color.blue);

		float foodDistNorth = 0.0f;
		float foodDistNorthEast = 0.0f;
		float foodDistEast = 0.0f;
		float foodDistSouthEast = 0.0f;
		float foodDistSouth = 0.0f;
		float foodDistSouthWest = 0.0f;
		float foodDistWest = 0.0f;
		float foodDistNorthWest = 0.0f;

		float wallDistNorth = 0.0f;
		float wallDistNorthEast = 0.0f;
		float wallDistEast = 0.0f;
		float wallDistSouthEast = 0.0f;
		float wallDistSouth = 0.0f;
		float wallDistSouthWest = 0.0f;
		float wallDistWest = 0.0f;
		float wallDistNorthWest = 0.0f;

		float bodyDistNorth = 0.0f;
		float bodyDistNorthEast = 0.0f;
		float bodyDistEast = 0.0f;
		float bodyDistSouthEast = 0.0f;
		float bodyDistSouth = 0.0f;
		float bodyDistSouthWest = 0.0f;
		float bodyDistWest = 0.0f;
		float bodyDistNorthWest = 0.0f;

		Vector3 north = Vector3.forward;
		Vector3 northEast = Quaternion.Euler(0, 45, 0)*Vector3.forward;
		Vector3 east = Vector3.right;
		Vector3 southEast = Quaternion.Euler(0, 45, 0)*Vector3.right;
		Vector3 south = -Vector3.forward;
		Vector3 southWest = Quaternion.Euler(0, 45, 0)*-Vector3.forward;
		Vector3 west = -Vector3.right;
		Vector3 northWest = Quaternion.Euler(0, 45, 0)*-Vector3.right;

		RaycastHit[] hits;
    hits = Physics.RaycastAll(transform.position, north, scaleFactor*10);

    for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistNorth = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistNorth = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistNorth = hits[i].distance;
			}
		}
		hits = Physics.RaycastAll(transform.position, northEast, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistNorthEast = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistNorthEast = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistNorthEast = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, east, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistEast = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistEast = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistEast = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, southEast, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistSouthEast = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistSouthEast = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistSouthEast = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, south, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistSouth = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistSouth = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistSouth = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, southWest, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistSouthWest = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistSouthWest = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistSouthWest = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, west, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistWest = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistWest = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistWest = hits[i].distance;
			}
		}

		hits = Physics.RaycastAll(transform.position, northWest, scaleFactor*10);

		for (int i = 0; i < hits.Length; i++){
			if(hits[i].collider.name.StartsWith("Food")){
				foodDistNorthWest = 1.0f;
			}
			if(hits[i].collider.name.StartsWith("Tail")){
				bodyDistNorthWest = hits[i].distance;
			}
			if(hits[i].collider.name.StartsWith("Wall")){
				wallDistNorthWest = hits[i].distance;
			}
		}

		float[] z = {relativeFoodPosition.x,relativeFoodPosition.z,foodDistNorth,foodDistEast,foodDistSouth,foodDistWest,wallDistNorth/scaleFactor,wallDistEast/scaleFactor,wallDistSouth/scaleFactor,wallDistWest/scaleFactor,bodyDistNorth/scaleFactor,bodyDistEast/scaleFactor,bodyDistSouth/scaleFactor,bodyDistWest/scaleFactor};

		for(int h=1;h<networkStructure.Length;h++){
			a = new float[networkStructure[h]];

			for(int i = 0; i<networkStructure[h]; i++){
				a[i] = 0f;
			}

			for(int i = 0; i<networkStructure[h]; i++){
				for(int j = 0; j<networkStructure[h-1]; j++){
					a[i] += z[j]*weights[h-1][i,j];
				}
				a[i] += biases[h-1];
			}

			z = new float[networkStructure[h]];

			for(int i = 0; i<networkStructure[h]; i++){
				z[i] = sigmoid(a[i]);
			}
		}

		float total = 0.0f;

		for(int i=1; i<networkStructure[networkStructure.Length - 1]; i++){
			total += Mathf.Exp(z[i]);
		}

		for(int i=1; i<networkStructure[networkStructure.Length - 1]; i++){
			z[i] = Mathf.Exp(z[i])/total;
		}

		int largest = 0;
		float largestvalue = z[largest];

		for(int i=1; i<networkStructure[networkStructure.Length - 1]; i++){
			if(z[i]>largestvalue){
				largest = i;
				largestvalue = z[i];
			}
		}

		switch(largest){
			case 0:
				dir = Vector3.right;
				break;
			case 1:
				dir = Vector3.forward;
				break;
			case 2:
				dir = -Vector3.right;
				break;
			case 3:
				dir = -Vector3.forward;
				break;
		}

		if (Input.GetKey(KeyCode.RightArrow)){
        dir = Vector3.right;
		}
    else if (Input.GetKey(KeyCode.DownArrow)){
        dir = -Vector3.forward;
		}
    else if (Input.GetKey(KeyCode.LeftArrow)){
        dir = -Vector3.right;
		}
    else if (Input.GetKey(KeyCode.UpArrow)){
        dir = Vector3.forward;
		}
		else if (Input.GetKey(KeyCode.K)){
			alive = false;
		}

}

	void appendTail(Color colorCollider){
		Vector3 spawnPos;
		if(Snake.Count>0){
			spawnPos = Snake[Snake.Count - 1].transform.position;
		}
		else{
			spawnPos = transform.position - dir;
		}

		GameObject tail = Instantiate(Tail,spawnPos,Quaternion.identity) as GameObject;
		tail.GetComponent<Renderer>().material.color = colorCollider;

			if(!Snake.Contains(tail)){
				Snake.Add(tail);
			}
			score += 1;
			scoreText.text = score + "";
	}

	void OnTriggerEnter(Collider collider) {
				if(collider.name.StartsWith("Food")){
					Color colorCollider = collider.GetComponent<Renderer>().material.color;
					GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().Spawn();
					Destroy(collider.gameObject);
					timeLastEaten = Time.time;
					appendTail(colorCollider);
				}
				else if(collider.name.StartsWith("Tail")){
					Vector3 foodPosition = GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().food.transform.position;
					Vector3 relativeFoodPosition = foodPosition - transform.position;
					distanceFromFood = relativeFoodPosition.sqrMagnitude;
					collidedWithTail = true;
					alive = false;
				}
				else if(collider.name.StartsWith("Wall")){
					Vector3 foodPosition = GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().food.transform.position;
					Vector3 relativeFoodPosition = foodPosition - transform.position;
					distanceFromFood = relativeFoodPosition.sqrMagnitude;
					collidedWithWall = true;
					alive = false;
				}
				else if(collider.name.StartsWith("Head")){
					Vector3 foodPosition = GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().food.transform.position;
					Vector3 relativeFoodPosition = foodPosition - transform.position;
					distanceFromFood = relativeFoodPosition.sqrMagnitude;
					collidedWithHead = true;
					alive = false;
				}
    }

	void updateSnake(){
		TicTime++;
		if(TicTime>=TicTimeLimit){
			if(Snake.Count>0){
				for(int j = Snake.Count - 1;j>0;j--){
					if(j>0){
						Snake[j].transform.position = Snake[j-1].transform.position;
					}
				}
				Snake[0].transform.position = transform.position;
			}
			TicTime = 0;

			// Vector3 desiredPosition = transform.position + dir;
			// transform.position = Vector3.Lerp(transform.position,desiredPosition,1f);
			//transform.rotation = Quaternion.LookRotation(dir);
			transform.position += dir;

			if(Snake.Count > 0 && previousDir + dir == Vector3.zero){
				collidedWithTail = true;
				alive = false;
			}

			previousDir = dir;
		}
	}

	float relu(float activation){
		return Mathf.Max(0,activation);
	}

	float sigmoid(float activation){
		return 1/(1+Mathf.Exp(activation));
	}

	float tanh(float activation){
		return 2*sigmoid(2*activation) - 1;
	}

	// Use this for initialization
	void Start () {
		Vector2 map = GameObject.Find("Map").GetComponent<MapGenerator>().mapSize;
		scaleFactor = Mathf.Sqrt(Mathf.Pow((float) map.x,2) + Mathf.Pow((float) map.y,2));
		float mapx = (float) map.x;

		scoreText = GameObject.Find("Score/Canvas/scoreText").GetComponent<UnityEngine.UI.Text>();
		startTime = Time.time;
		timeLastEaten = Time.time;
		alive = true;
		collidedWithWall = false;
		collidedWithTail = false;

		score = 0;
		scoreText.text = score + "";

		List<Vector3> vecs = new List<Vector3>();
		vecs.Add(Vector3.right);
		vecs.Add(-Vector3.right);
		vecs.Add(Vector3.forward);
		vecs.Add(-Vector3.forward);
		dir = vecs[Random.Range(0,4)];



		transform.GetComponent<Renderer>().material.color = color;
		appendTail(color);
		appendTail(color);
		appendTail(color);
		score -= 3;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if(Time.time - timeLastEaten > killTime){
			alive = false;
		}
		Inputs();
		updateSnake();
	}
}

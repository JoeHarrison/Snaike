using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbsoluteSnake : MonoBehaviour, ISnake {

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
		Vector3 relativeFoodPosition = (foodPosition - ownPosition)/scaleFactorFood;

		Debug.DrawRay(transform.position, (foodPosition - ownPosition), Color.blue);

		Ray wallf = new Ray(transform.position,Vector3.forward);
		Ray wallr = new Ray(transform.position,Vector3.right);
		Ray walll = new Ray(transform.position,-Vector3.right);
		Ray wallb = new Ray(transform.position,-Vector3.forward);
		RaycastHit hit;

		float distf = 0.0f;
		float distr = 0.0f;
		float distl = 0.0f;
		float distb = 0.0f;

		float distff = 0.0f;
		float distrf = 0.0f;
		float distlf = 0.0f;
		float distbf = 0.0f;

		if(Physics.Raycast(wallf,out hit)){
			if(hit.collider.name.StartsWith("Food")){
				distff = 1.0f;
				Debug.DrawRay(transform.position, Vector3.forward*hit.distance, Color.green);
			}
			else{
				distf = hit.distance/scaleFactor;
				Debug.DrawRay(transform.position, Vector3.forward*hit.distance, Color.red);
			}
		}
		if(Physics.Raycast(wallr,out hit)){
			if(hit.collider.name.StartsWith("Food")){
				distrf = 1.0f;
				Debug.DrawRay(transform.position, Vector3.right*hit.distance, Color.green);
			}
			else{
				distr = hit.distance/scaleFactor;
				Debug.DrawRay(transform.position, Vector3.right*hit.distance, Color.red);
			}
		}
		if(Physics.Raycast(walll,out hit)){
			if(hit.collider.name.StartsWith("Food")){
				distlf = 1.0f;
				Debug.DrawRay(transform.position, -Vector3.right*hit.distance, Color.green);
			}
			else{
				distl = hit.distance/scaleFactor;
				Debug.DrawRay(transform.position, -Vector3.right*hit.distance, Color.red);
			}
		}
		if(Physics.Raycast(wallb,out hit)){
			if(hit.collider.name.StartsWith("Food")){
				distbf = 1.0f;
				Debug.DrawRay(transform.position, -Vector3.forward*hit.distance, Color.green);
			}
			else{
				distb = hit.distance/scaleFactor;
				Debug.DrawRay(transform.position, -Vector3.forward*hit.distance, Color.red);
			}
		}

		//relativeFoodPosition.x,relativeFoodPosition.z
		//distff,distrf,distlf,distbf,
		//List<float> inputs = new List<float>() {dir.x,dir.z,relativeFoodPosition.x,relativeFoodPosition.z,distff,distrf,distlf,distbf,distf,distr,distl,distb};
		float[] z = {dir.x,dir.z,relativeFoodPosition.x,relativeFoodPosition.z,distff,distrf,distlf,distbf,distf,distr,distl,distb};
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
			//inputs<-z
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
		scaleFactor = (float) map.x - 2.5f + 0.05f;
		float mapx = (float) map.x;
		scaleFactorFood = Mathf.Sqrt(Mathf.Pow((mapx - 3f),2.0f) + Mathf.Pow((mapx - 3f),2.0f));

		scoreText = GameObject.Find("Score/Canvas/scoreText").GetComponent<UnityEngine.UI.Text>();
		startTime = Time.time;
		timeLastEaten = Time.time;
		alive = true;
		collidedWithWall = false;
		collidedWithTail = false;

		score = 0;
		scoreText.text = score + "";
		dir = Vector3.right;

		transform.GetComponent<Renderer>().material.color = color;
		appendTail(color);
		appendTail(color);
		appendTail(color);
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

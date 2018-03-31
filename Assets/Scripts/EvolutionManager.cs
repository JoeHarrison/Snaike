using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EvolutionManager : MonoBehaviour {
	private int top = 4;
	private int bottom = -4;
	private int right = 4;
	private int left = -4;
	private Vector2 map;

	public int populationSize;
	public int generations;
	public int currentSnakeNumber;
	public int currentGeneration;
	public int randoms;
	public int elites;
	public float smoothing = 1f;

	public int[] networkStructure;
	public string activationFunction;

	public float mutationRate;
	public float mutationProbability;
	public float crossOverProbability;

	public float highestFitness = 0f;
	public float best = 0f;
	public float bestCurrentGeneration = 0f;
	public float bestAverage = 0f;
	public float averageCurrentGeneration = 0f;
	public populationEntity bestSnake;
	public float currentTotal = 0f;

	public populationEntity[] population;

	public GameObject snakePrefab;
	private GameObject currentSnake;

	void Start () {
		Vector2 map = GameObject.Find("Map").GetComponent<MapGenerator>().mapSize;
		top = (int) map.y / 2 - 1;
		bottom = (int) -map.y / 2 + 1;
		right = (int) map.x / 2 - 1;
		left = (int) -map.x / 2 + 1;

		population = new populationEntity[populationSize];

		for(int i = 0; i<populationSize; i++){
			population[i] = new populationEntity(networkStructure);
		}

		currentSnakeNumber = 0;
		currentGeneration = 0;
		currentSnake = Instantiate(snakePrefab, new Vector3(0,0.5f,0), Quaternion.identity);
		currentSnake.GetComponent<ISnake>().Initialize(population[currentSnakeNumber].weights,population[currentSnakeNumber].biases,population[currentSnakeNumber].networkStructure,population[currentSnakeNumber].color);
	}

	// Update is called once per frame
	void Update () {
		if(!currentSnake.GetComponent<ISnake>().isAlive()){

			population[currentSnakeNumber].score = currentSnake.GetComponent<ISnake>().getScore();
			population[currentSnakeNumber].timeAlive = currentSnake.GetComponent<ISnake>().getTimeAlive();
			population[currentSnakeNumber].collidedWithWall = currentSnake.GetComponent<ISnake>().isCollidedWithWall();
			population[currentSnakeNumber].distanceFromFood = currentSnake.GetComponent<ISnake>().getDistanceFromFood();
			population[currentSnakeNumber].fitness = getSurvivalScore(population[currentSnakeNumber]);

			currentTotal += population[currentSnakeNumber].score;
			averageCurrentGeneration = currentTotal/(currentSnakeNumber+1);

			if(population[currentSnakeNumber].fitness > highestFitness){
				highestFitness = population[currentSnakeNumber].fitness;
				bestSnake = population[currentSnakeNumber];
			}

			if(population[currentSnakeNumber].score > best){

				best = population[currentSnakeNumber].score;
			}
			if(population[currentSnakeNumber].score > bestCurrentGeneration){
				bestCurrentGeneration = population[currentSnakeNumber].score;
			}

			foreach (GameObject tailPart in currentSnake.GetComponent<ISnake>().getSnake()){
				Destroy(tailPart);
			}
			Destroy(currentSnake);
			GameObject food = GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().food;
			Destroy(food);
			GameObject.Find("FoodSpawner").GetComponent<FoodSpawner>().Spawn();

			currentSnake = nextInPopulation();
			currentSnake.GetComponent<ISnake>().Initialize(population[currentSnakeNumber].weights,population[currentSnakeNumber].biases,population[currentSnakeNumber].networkStructure,population[currentSnakeNumber].color);
		}
	}

	float getSurvivalScore(populationEntity popEnt){
		float score = Mathf.Pow(2,popEnt.score) + Mathf.Pow(popEnt.timeAlive,2);
		return score;
	}

	GameObject nextInPopulation(){
		if(currentSnakeNumber != populationSize - 1){
			currentSnakeNumber += 1;
		}
		else{
			currentGeneration += 1;
			currentSnakeNumber = 0;

			float totalScore = 0f;
			bestCurrentGeneration = 0f;

			if(averageCurrentGeneration>bestAverage){
				bestAverage = averageCurrentGeneration;
			}

			averageCurrentGeneration = 0f;
			currentTotal = 0f;

			foreach(populationEntity pop in population.OrderByDescending(x=>getSurvivalScore(x))){
				totalScore += getSurvivalScore(pop);
			}

			float[] probabilities = new float[populationSize];

			for(int i = 0; i< populationSize; i++){
				//probabilities[i] = linearRanking(populationSize,i,1.75f);
				//Smoothing
				probabilities[i] = (getSurvivalScore(population[i]) + smoothing)/(totalScore + populationSize*smoothing);
			}

			for(int i = elites; i < populationSize-randoms; i++){
				populationEntity[] sel = selection(population, probabilities);
				populationEntity[] cv = crossover2(sel[0],sel[1]);
				population[i] = mutate(cv[0]);
			}

			for(int i = populationSize-randoms; i<populationSize; i++){
				population[i] = new populationEntity(networkStructure);
			}

			for(int i = 0; i<elites; i++){
				population[i] = bestSnake;
			}
		}

		int xc = (int) Random.Range(left,right);
		int z = (int) Random.Range(bottom,top);
		// xc = 0;
		// z = 0;
		return Instantiate(snakePrefab, new Vector3(xc,0.5f,z), Quaternion.identity);
	}

	float linearRanking(int mu, int i, float s){
		i = mu - i - 1;
		return (2-s)/mu + 2*i*(s-1)/(mu*(mu-1));
	}

	int getRandomIntWithProbability(float[] probabilities){
		float p = Random.Range(0f,1.0f);
		float cumulative = 0.0f;
		int r = 0;
		for (int i = 0; i < populationSize; i++){
    	cumulative += probabilities[i];
    	if (p < cumulative){
        r = i;
        break;
    	}
		}
		return r;
	}

	populationEntity[] selection(populationEntity[] pop, float[] probabilities){

		populationEntity[] selection = new populationEntity[2];

		selection[0] = pop[getRandomIntWithProbability(probabilities)];
		selection[1] = pop[getRandomIntWithProbability(probabilities)];

		return selection;
	}

	populationEntity[] crossover(populationEntity left, populationEntity right){
		int[] leftNetworkStructure = left.networkStructure;
		populationEntity[] crossover = new populationEntity[2];

		for(int h=1;h<leftNetworkStructure.Length;h++){
			for(int i = 0; i<leftNetworkStructure[h]; i++){
				for(int j = 0; j<leftNetworkStructure[h-1]; j++){
					if(Random.Range(0f,1f)<crossOverProbability){
						float tempw1 = left.weights[h-1][i,j];
						left.weights[h-1][i,j] = right.weights[h-1][i,j];
						right.weights[h-1][i,j] = tempw1;
					}
				}
			}
		}

		crossover[0] = left;
		crossover[1] = right;

		//bias?

		return crossover;
	}

	populationEntity[] crossover2(populationEntity left, populationEntity right){
		int[] leftNetworkStructure = left.networkStructure;
		populationEntity[] crossover = new populationEntity[2];



		for(int h=1;h<leftNetworkStructure.Length;h++){
			float randi = Random.Range(0,leftNetworkStructure[h-1]);
			float randj = Random.Range(0,leftNetworkStructure[h]);

			for(int i = 0; i<leftNetworkStructure[h]; i++){
				for(int j = 0; j<leftNetworkStructure[h-1]; j++){
					if (i< randi || (i==randi && j<=randj)) {
						float tempw1 = left.weights[h-1][i,j];
						left.weights[h-1][i,j] = right.weights[h-1][i,j];
						right.weights[h-1][i,j] = tempw1;
					}
				}
			}
		}

		crossover[0] = left;
		crossover[1] = right;

		//bias?

		return crossover;
	}

	populationEntity mutate(populationEntity populationEntity){
		int[] popNetworkStructure = populationEntity.networkStructure;

		for(int h=1;h<popNetworkStructure.Length;h++){
			for(int i = 0; i<popNetworkStructure[h]; i++){
				for(int j = 0; j<popNetworkStructure[h-1]; j++){
					if(Random.Range(0f,1f)<mutationProbability){
						populationEntity.weights[h-1][i,j] += gaussian(0f,mutationRate)/5;
						//clamp?
					}
				}
			}
		}
		//Bias?

		return populationEntity;
	}

	float gaussian(float mu,float sigma){
		float u = Random.Range(0f,1f);
		float v = Random.Range(0f,1f);
		float z = Mathf.Sqrt(-2f * Mathf.Log(u)) * Mathf.Sin(2f * Mathf.PI * v);
		return mu + sigma*z;
	}
}

[System.Serializable]
public struct populationEntity{
	public List<float[,]> weights;
	public int[] networkStructure;

	public List<float> biases;

	public int score;
	public float timeAlive;
	public float fitness;
	public float distanceFromFood;
	public bool collidedWithWall;
	public bool collidedWithTail;
	public bool collidedWithHead;

	public Color color;

	public populationEntity(int[] networkStructure){
		color = new Color(Random.value, Random.value, Random.value, 1.0f );

		fitness = 0.0f;
		score = 0;
		timeAlive = 0f;
		collidedWithWall = false;
		collidedWithTail = false;
		collidedWithHead = false;
		distanceFromFood = float.MaxValue;





		weights = new List<float[,]>();
		this.networkStructure = networkStructure;

		biases = new List<float>();

		for(int i=1;i<networkStructure.Length;i++){
			biases.Add(1.0f);
			weights.Add(init_weights(networkStructure[i-1],networkStructure[i]));
		}
	}

	public float[,] init_weights(int input_size,int output_size){
		float[,] w = new float[output_size,input_size];

		for(int i = 0; i<output_size; i++){
			for(int j = 0; j<input_size; j++){
				w[i,j] = Random.Range(-1f,1f);
			}
		}
		return w;
	}

	float gaussian(float mu,float sigma){
		float u = Random.Range(0f,1f);
		float v = Random.Range(0f,1f);
		float z = Mathf.Sqrt(-2f * Mathf.Log(u)) * Mathf.Sin(2f * Mathf.PI * v);
		return mu + sigma*z;
	}
}

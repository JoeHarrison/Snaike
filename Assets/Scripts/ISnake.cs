using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface ISnake{
	bool isAlive();

	int getScore();

	float getTimeAlive();

	bool isCollidedWithWall();

	float getDistanceFromFood();

	List<GameObject> getSnake();

	void Initialize(List<float[,]> weights, List<float> biases, int[] networkStructure, Color color);
}

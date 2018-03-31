using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private Transform target;
	public Vector3 offset;
	public Vector3 angle;

	public float smoothSpeed = 0.5f;

	void FixedUpdate(){
		if(target == null){
			target = GameObject.FindGameObjectsWithTag("Snake")[0].transform;
			//transform.Rotate(angle.x,angle.y,angle.z);
		}
		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position,desiredPosition,smoothSpeed);
		transform.position = smoothedPosition;

	}


}

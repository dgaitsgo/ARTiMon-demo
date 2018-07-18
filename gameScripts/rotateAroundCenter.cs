using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateAroundCenter : MonoBehaviour {

	Vector3 dir;
	float offset;
	float immAngle;
	float targetAngle;

	public GameObject goodArrow;
	public GameObject badArrow;

	void Update () {
		dir = GameObject.Find ("heliPadImage").transform.position - GameObject.Find("Player").transform.position;
		dir.y = 0;
		offset = Vector3.Angle (dir, new Vector3(0, 0, 1));
		targetAngle = GameObject.Find("Player").transform.rotation.eulerAngles.y + offset;
		if (targetAngle < 30 || targetAngle > 330) {
			goodArrow.SetActive (true);
			badArrow.SetActive (false);
		}
		else {
			goodArrow.SetActive (false);
			badArrow.SetActive (true);
		}
		goodArrow.transform.eulerAngles = new Vector3(0, 0, -targetAngle);
		badArrow.transform.eulerAngles = new Vector3(0, 0, -targetAngle);
	}
}
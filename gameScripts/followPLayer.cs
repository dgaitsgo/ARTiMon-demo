using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPLayer : MonoBehaviour {

	GameObject Player;
	void Start () {
		Player = GameObject.Find ("Player");
	}
		
	void Update () {
		Vector3 transform_imm = Player.transform.position;
		transform.position = new Vector3 (transform_imm.x, transform_imm.y, transform_imm.z + 5);
	}
}

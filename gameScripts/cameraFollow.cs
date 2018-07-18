using UnityEngine;
using System.Collections;

public class cameraFollow : MonoBehaviour {
	public Transform target;
	public float looksmooth = 0.09f;
	public Vector3 offsetFromTarget = new Vector3 (0, 6, -14);
	public float xTilt = 10;

	Vector3 destination = Vector3.zero;
	movement move;
	float rotateVel = 0;

	void Start()
	{
		SetCameraTarget (target);
	}

	void SetCameraTarget(Transform t)
	{
		target = t;
		if (target != null) {
			if (target.GetComponent<movement> ()) {
				move = target.GetComponent<movement> ();
			} else
				Debug.LogError ("snail");
		} else
			Debug.LogError ("camera needs a target");
	}

	void LateUpdate()
	{
		MoveToTarget ();
		LookAtTarget ();
	}

	void MoveToTarget()
	{
		destination = move.TargetRotation * offsetFromTarget;
		destination += target.position;
		transform.position = destination;
	}

	void LookAtTarget()
	{
		float eulerYAngle = Mathf.SmoothDampAngle (transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, looksmooth);
		transform.rotation = Quaternion.Euler (transform.eulerAngles.x, eulerYAngle, 0);
	}
}

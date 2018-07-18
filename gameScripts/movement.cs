using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

public class movement : MonoBehaviour
{
	public float speed = 6.0f;
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	private float moveHorizontal;
	public float rotateVel = 100;
	private Quaternion targetRotation;
	private CharacterController cc;
	private Vector3 moveDirection;
	public Animator anim;
	private float lengthOfTime;
	private float sinceFalse = 0;
	private float sinceTrue = 0;
	private bool motion;
	public float delta = 0;

	public class Timer
	{
		public bool		motion;
		public float 	lengthOfTruth;
		public float	timeStartTrue;
		public float 	timeStartFalse;
	}

	Timer timer;

	public Quaternion TargetRotation
	{
		get { return targetRotation; }
	}

	void Start ()
	{
		transform.position = new Vector3 (1849.29f, 50.39f, 83.36f);
		anim = GetComponent<Animator> ();
		anim.SetFloat ("duration", 0);
		cc = GetComponent<CharacterController>();
		targetRotation = transform.rotation;
		timer = new Timer();
		timer.motion = false;
	}

	void GetInput()
	{
		moveHorizontal = Input.GetAxis ("Horizontal");
	}

	void Turn()
	{
		targetRotation *= Quaternion.AngleAxis (rotateVel * moveHorizontal * Time.deltaTime, Vector3.up);
		transform.rotation = targetRotation;
	}

	void Update ()
	{
		foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(kcode))
				Debug.Log("KeyCode down: " + kcode);
		}
		GetInput ();
		Run ();
		Turn ();
	}

	void	setTimer(Timer timer, bool state)
	{
		timer.motion = state;
		if (timer.motion == true)
			timer.timeStartTrue = Time.realtimeSinceStartup - delta;
		else {
			timer.timeStartFalse = Time.realtimeSinceStartup;
		}
	}

void Run() {
	movingFor = 0;
	stillFor = 0;
	float forwardMove = Input.GetAxis ("Vertical");
	if (move > 0) {
		moveDirection = new Vector3 (0, 0, move);
		moveDirection = transform.TransformDirection (moveDirection);
		moveDirection *= speed;
		if (timer.motion != true)
			setTimer (timer, true);
		movingFor = Time.realtimeSinceStartup - timer.movingFor;
		delta = Math.Max(0, movingFor - stillFor);
		anim.SetFloat ("duration", movingFor);
	} else {
		if (timer.motion == true) {
			timer.movingFor = Time.realtimeSinceStartup - timer.movingFor;
			setTimer (timer, false);
		}
		anim.SetFloat ("duration", 0);
		stillFor = (Time.realtimeSinceStartup - timer.timeStartFalse);
		delta = Math.Max(0, timer.movingFor - stillFor);
	}
	moveDirection.y -= gravity * Time.deltaTime;
	cc.Move (moveDirection * Time.deltaTime);
}
}
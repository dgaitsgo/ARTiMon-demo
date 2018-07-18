using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;

public class captors : MonoBehaviour
{
	movement s_movement;
	socket   s_socket;

	//ext. classes
	public helpers	helpers;
	public socket	socket;
	public text		text;

	/***************** class Definitions ***********/
	public enum e_range
	{
		MIN, MAX
	}

	public class BioSensorsRanges
	{
		public Vector2 heartBPM;
		public Vector2 bodyTemp;
		public Vector2 glucose;
	}

	public class BioSensors
	{
		//boolsw
		public bool			enemySpotted;
		public bool			athzToCont;
		public bool			flex;

		//vals
		public float		heartBPM;
		public float 		bodyTemp;
		public float		oxygen;
		public float 		glucose;
		public float 		hydration;
		public int			flexions;

		public BioSensors()
		{
			enemySpotted = false;
			athzToCont = true;
			flex = false;

			heartBPM = 40.0f;
			bodyTemp = 37.0f;
			oxygen = 100.0f;
			glucose = 85.0f;
			hydration = 100.0f;
			flexions = 0;
		}
	}

	public class MinsAndMaxes
	{
		public float 		minBodyTemp = 34.0f;
		public float 		maxBodyTemp = 40.0f;
	
		public float		minHeartBPM = 49.0f;
		public float		maxHeartBPM = 54.0f;

		public float 		minGlucose = 90.0f;
		public float 		maxGlucose = 95.0f;

		public float 		minOxygen = 90.0f;
	}

	/******************** global Variables ********/

	//enemy
	public 	GameObject 			Enemy;
	private float				minEnemyDistance = 30;

	//time
	private float				currTime;
	private float				lastReset;
	private	int					nextTime = 0;
	private int					interval = 1;

	//structs
	private	BioSensorsRanges	range 	= new BioSensorsRanges();
	private	BioSensors 			state 	= new BioSensors();
	private MinsAndMaxes		mams 	= new MinsAndMaxes();

	//biosensor variables
	private Animator			anim;
	private bool				inWater = false;
	public int					motionState;
	//float						distanceTravelled = 0.0f;
	//bool						fullStep = false;

	//gluouse
	private float 				flexionsSinceLastDrink = 0;
	private float 				flexionsSinceLastMeal = 0;
	private	float 				randomGlucoseLevel;
	private float 				mealBoost = 30.0f;

	private float getTime()
	{
		return (Time.time - lastReset);
	}

	private void resetTime()
	{
		nextTime = 0;
		interval = 1;
		lastReset = Time.time;

	}

	public bool checkEnemyDistance()
	{
		GameObject current = GameObject.Find ("Enemy_instance");
		int i = 0;
		while (current != null)
		{
			float distance = Vector3.Distance (transform.position, current.transform.position);
			if (distance < minEnemyDistance)
				return (true);
			i++;
			current = GameObject.Find("Enemy_instance" + i);
		}
		return (false);
	}

	IEnumerator respawn(Collider other, int seconds) {
		yield return new WaitForSeconds(seconds);
		other.gameObject.SetActive (true);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag ("food")) {
			other.gameObject.SetActive (false);
			range.glucose[0] += mealBoost;
			range.glucose[1] += mealBoost;
			respawn (other, 7);
		}
		if (other.gameObject.CompareTag ("water")) {
			other.gameObject.SetActive (false);
			respawn(other, 7);
			state.hydration = state.hydration + 3;
		}
	}

	void OnTriggerStay(Collider other) {
			if (other.gameObject.CompareTag ("lake")) {
			inWater = true;
		}
	}

	void Reset()
	{
		transform.position = new Vector3 (1849.29f, 50.39f, 83.36f);
		//resetTime ();
		range.heartBPM[0] = mams.minHeartBPM;
		range.heartBPM[1] = mams.maxHeartBPM;
		range.bodyTemp[0] = mams.minBodyTemp;
		range.bodyTemp[1] = mams.maxBodyTemp;
		range.glucose[0] = mams.minGlucose;
		range.glucose[1] = mams.maxGlucose;

		state.oxygen = 100;
		state.hydration = 100;
		state.flexions = 0;
	}

	//initialization
	void Start ()
	{
		helpers = GetComponent<helpers>();
		socket = GetComponent<socket>();
		text = GetComponent<text>();
		anim = GetComponent<Animator>();
		s_movement = GetComponent<movement>();
		s_socket = GetComponent<socket>();
	
		range.heartBPM[0] = mams.minHeartBPM;
		range.heartBPM[1] = mams.maxHeartBPM;
		range.bodyTemp[0] = mams.minBodyTemp;
		range.bodyTemp[1] = mams.maxBodyTemp;
		range.glucose[0] = mams.minGlucose;
		range.glucose[1] = mams.maxGlucose;
	}

	public float calcBPM(float duration)
	{
		return (192.0f / (1.0f + 3.1f * (float)Math.Pow((double)Math.E, (double)(-0.4f * duration))));
	}

	public float calcBodyTemp(float elevation, float hydration, float heartRate)
	{
		return (((100.0f - hydration) / 10.0f) + (float)-Convert.ToSingle (inWater) + (heartRate * 0.0045f) + mams.minBodyTemp
			+ (float)(2.8 / 1 + Math.Pow (1.4, Math.Log(50) - (double)elevation) * -.045f));
	}

	public float calcOxygen(float elevation, float heartRate)
	{
		return (-(heartRate * 0.0045f) + mams.minOxygen
			+ (float)(10.0 / (1.0 + (Math.Pow (1.15, (Math.Log(50) - (double)elevation)) * -0.0095))));
	}

	int getFlexionsFromAnimation()
	{
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Basic_Walk_01"))
			return (1);
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Basic_Run_02"))
			return (2);
		return (0);
	}

	void Update ()
	{
		currTime = getTime();

		if (Input.GetKeyDown(KeyCode.R))
		{
			Reset();
		}

		if (currTime >= nextTime)
		{
			state.enemySpotted = checkEnemyDistance();
			state.athzToCont = !state.enemySpotted;
			state.hydration -= 0.02f + flexionsSinceLastDrink * 0.02f;
			state.glucose -= 0.02f + flexionsSinceLastMeal * 0.04f;
			state.heartBPM = calcBPM(s_movement.res);
			state.bodyTemp = calcBodyTemp(transform.position.y, state.hydration, state.heartBPM);
			motionState = getFlexionsFromAnimation ();
			if (motionState == 0) {
				state.flexions = 0;
				state.flex = false;
			}
			else {
				state.flex = true;
				state.flexions += motionState;
				flexionsSinceLastDrink += motionState;
				flexionsSinceLastMeal += motionState;
			}
			state.oxygen = calcOxygen (transform.position.y, state.heartBPM);

			String[] lowPrescData = text.handleUIText (state, currTime);
			s_socket.writeToSocket (lowPrescData [0] + " " +
			Convert.ToInt32 (state.enemySpotted).ToString () + " " +
			Convert.ToInt32 (state.athzToCont).ToString () + " " +
			Convert.ToInt32 (motionState != 0).ToString () + " " +
			lowPrescData [1] + " " +
			lowPrescData [2] + " " +
			lowPrescData [3] + " " +
			lowPrescData [4] + " " +
			lowPrescData [5] + " " +
			state.flexions.ToString ());
			nextTime += interval;
		}
	}
}
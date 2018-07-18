# ARTiMon - Demo
![Image](https://i.imgur.com/rTcvw9Y.png)


Partnership École42-STRATE-CEA


Core team

David Gaitsgory, Developer, dgaitsgory@student.42.fr
Paul Gibert, Designer, pd.gibert@strate.design
Olivier Tousche, Designer, o.tousche@strate.design
Anaëlle Zouari, Developer, aazouari@student.42.fr


Under the direction

Aline Caranicolas, Industry Partnerships, aline.caranicolas@cea.fr
David Mercier, Head of Innovation at LIST Institute, CEA, david.mercier@cea.fr
Nicolas Rapin, Researcher in Formal Methods, nicolas.rapin@cea.fr


## Background
As the world leader in patent filings amongst any public research organisation, the CEA is a vital bridge between cutting edge applied research and industry implementations. In order to attract new investment and public interest, the organisation displays its technical prowess in a mind blowing demo room in Saclay, France. Students at the Srate School of Design and École42 were brought together to help advance their outreach mission by creating demos that hook potential industry partners and popularise their scientific research. Our core team created a demo for ARTiMon.


## What is ARTiMon?
> Artimon is a technology that serves two purposes. Specify behaviours in real time and then analyse them. Artimon offers a language to easily express temporality. It allows the conceiver, from a very high level, to express temporal constraints in a declarative, textual language that is quite close to natural language. Therefore, it allows the synthesis of automatic monitoring of a system and decide if it respects the constraints in real time.

*- Nicolas Rapin, Principle designer of ARTiMon*


![Image](https://i.imgur.com/7qg27fh.png)


Monitoring the behavior in real-time of mission-critical systems is a complex task. It can often lead to code bases so dense, they become hard to test for dependable behavior. This is more often than not because languages are not designed to explicitly express temporality despite being a fundamental way humans conceive rules and constraints. ARTiMon shifts the field by offering a language where its primary operators are temporal.


## Our Mission
Our mission was to communicate to decision makers and the public the major steps the CEA has taken in making the real-time monitoring of critical systems more ergonomic in an engaging demo.


## Approach
After the exploration of several prototypes, we decided that the most effective way to give a user who is not versed in a niche technical subject would be an interactive game where they play a soldier undress distress and must safely get to an extraction point while managing their vitals. Users are explained the rules they will have to follow and shown them expressed in ARTiMon to compare the similarity. The user has three points of interaction: a screen where a third-person character is in a dynamic environment, an arcade style joystick to control the character and toggle through menus, and a wearable technology that allows them to receive alerts from ARTiMon.
## Stack


**ARTiMon**


ARTiMon is a new language with a set of unique symbols and operators to express temporal logic. Here is a simple example from the game of a variable that must be managed for successful completion.

```
*
//Here, we declare variables which will be monitored during the
//execution of the program 
heartRate
*
//Next, we declare contraints and their parameters related to what will
//be monitored. In this case, we declare a maximum, a range and a
//comparative value to describe when the heartRate is too high.
 
maxHeartRate = 'double_cst' 187
heartRateRange[2] heartRate maxHeartRate
elevatedHeartRate = '>' heartRateRange

*
//Finally its time declare what alerts will be produced when exceeding
//certain thresholds. Alerts escalate in degree if the our heartRate is
//elevated for 3, 7 and 10 seconds. This is the monitoring stage.
// hazards & warnings :green (default) - yellow, orange, red
() yellow_HeartRate = Top (G[0, 3] elevatedHeartRate)
() orange_HeartRate = (G[0, 7] elevatedHeartRate)
() red_heartRate = G[0, 10] elevatedHeartRate
```

These rules can be aggregated to create complex interdependent temporal rules without losing its declarative approach.


**Unity**


Creating a navigable environment for our demo meant game design of terrains and characters, game scripting to control movement, menus, cameras and assets, and to simulate captors and communicate their values to our ARTiMon device.


**Camera**


In order to make a camera follow an entity in Unity, we have to do two things. Move the camera to its target and actually 'point' at it.


In Unity, our target should have a getter that defines its movement

```csharp
move = target.GetComponent<movement> ();
```


Next we use its position and rotation to place our camera:

```csharp
/*
    offsetFromTarget world is a constant that gives a natural
    distance between the screen and the character.
*/
void MoveToTarget() {
	destination = move.TargetRotation * offsetFromTarget;
	destination += target.position;
	transform.position = destination;
}
```


Unity has a very flexible API that allows easy conversion between Euler angles and quaternions which should ultimately define transformations to avoid gimbal lock.

```csharp
void LookAtTarget() {
	float eulerYAngle = Mathf.SmoothDampAngle (transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, looksmooth);
	transform.rotation = Quaternion.Euler (transform.eulerAngles.x, eulerYAngle, 0);
}
```


**Character**


Our character does primarily three things: run, turn, and jump. We'll look at the script to make the character run.



```csharp

void Run() {
	movingFor = 0;
	stillFor = 0;
	//This is data from our controller, the forward angle of the Joystick
	float forwardMove = Input.GetAxis ("Vertical");
	if (move > 0) {
		moveDirection = new Vector3 (0, 0, move);
		moveDirection = transform.TransformDirection (moveDirection);
		moveDirection *= speed;
		//We have to keep track of how long the player has been moving
		//for our simulation - as the amount of time they've been running
		//or resting will effect their heart rate.
		if (timer.motion != true)
			setTimer (timer, true);
		movingFor = Time.realtimeSinceStartup - timer.movingFor;
		delta = Math.Max(0, movingFor - stillFor);
		anim.SetFloat ("duration", movingFor);
	} else {
		if (timer.motion == true) {
			timer.movingFor =
				Time.realtimeSinceStartup - timer.movingFor;
			setTimer (timer, false);
		}
		anim.SetFloat ("duration", 0);
		stillFor = (Time.realtimeSinceStartup - timer.timeStartFalse);
		delta = Math.Max(0, timer.movingFor - stillFor);
	}
        //We move the character a certain distance and multiply by
	//Time.deltaTime to make sure the update is independent of
	//frame rate.
	moveDirection.y -= gravity * Time.deltaTime;
	cc.Move (moveDirection * Time.deltaTime);
}
```


**Captors**


Multiple environmental variables can contribute to the vitals of our character as displayed in this function which takes into consideration the character's level of elevation, hydration and heartRate to calculate their body temperature.

```csharp
public float calcBodyTemp(float elevation, float hydration, float heartRate) {
    return (
	((100.0f - hydration) / 10.0f) +
	(float)-Convert.ToSingle (inWater) +
	(heartRate * 0.0045f) + mams.minBodyTemp +
	(float)(2.8 / 1 + Math.Pow (1.4, Math.Log(50) -
	(double)elevation) * -.045f));
}
```
While this may seem like just a bunch of magic numbers, and ultimately they are, they do come from a method. We first defined the variables we wanted to take into consideration that would effect the body temperature and then plotted them onto a graph with x as time. With a lot of tinkering, The idea was to find a curve that would be manageable for the player to respond to the fluctuations in their character's vitals and the alerts they might raise.


![Image](https://i.imgur.com/KEXRSy0.png)


**Communication**


In order to get data from the Unity game to our ARTiMon executable, we configured the wearable RaspberyPi to host a wifi network, connected the computer running the game to it and used tcp sockets to stream the data. When scripting in Unity with C#, we have access to .NET's Tcp API.



```csharp
public void	setupSendSocket() {
	//...setup of constants          
	sendSock.socket = new TcpClient(sendSock.host, sendSock.port);
	sendSock.stream = sendSock.socket.GetStream();
	sendSock.writer = new StreamWriter(sendSock.stream);
	sendSock.socketReady = true;
	...
	//error handling
}

//All of our data was a comma separated string
public void writeToSocket(String s)
{
	sendSock.writer.Write (s);
	sendSock.writer.Flush ();
}
```


**RaspberryPi**


![Image](https://i.imgur.com/BYrVNIJ.jpg)


Our RaspberryPi was configured to do several tasks: run the ARTiMon executable, host the network that would allow data transfer and be a wearable device a user can interact with through alerts and touch. To get data into ARTiMon, we launched an executable that included an interface to the monitoring system which ran as a child process:



```c
typedef struct s_artimon {
	int nb_atoms;
	int nb_sig;
	int nb_main;
	int nb_warn;
	unsigned int sigtab[2];
	double *hazard_detect_table;
	double *main_reset;
	double *warn_value_tab;
	notif_42 **notif_tab;
} t_artimon;
```


This interface was not a core feature of ARTiMon before our demo and we were thrilled not just show off the language but help advance it as well. After that, it was a matter of listening on the socket for updates and passing that information onto ARTiMon and our web server via JSON:



```c
while (1) {
	check_alerts(&send_site, &artimon, db);
	reader.bytes_read = recv(listen_unity.client_socket_desc, reader.buffer, BUFF_SIZE, 0);
	if (reader.bytes_read > 0) {
		reader.buffer[reader.bytes_read] = '\0';
		tokenize(&reader);
		unity_string_to_sensor_data(&reader, &sensor);
		sensor_data_to_artimon(&sensor, &artimon);
		char *dump = json_dumps(db, 0);
		write(send_site.client_socket_desc, dump, strlen(dump));
		free(dump);
		artimon_refresh_time(sensor.array[GLOBAL_TIME]);
	}
}
```


## Putting it all together


![Image](https://i.imgur.com/3prfLmH.png)


Here we can see our character in peril and the beautifully crafter joystick and menu button to control them.


![Image](https://i.imgur.com/6fiELJM.png)


A wearable, touch screen RaspberryPi to guide gamers on their journey.


![Image](https://i.imgur.com/Ggnqy3W.png)


An exhausted but unstoppable team. ❤️

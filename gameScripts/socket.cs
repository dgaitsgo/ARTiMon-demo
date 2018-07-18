using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;

public class socket : MonoBehaviour {

	public class Socket
	{	
		public bool socketReady;
		public TcpClient socket;
		public NetworkStream stream;
		public StreamWriter writer;
		public StreamReader reader;
		public String host;
		public Int32 port;
	}

	public	Socket 		sendSock;
	public	String 		host;
	private int 		sendPort;

	public void	setupSendSocket()
	{
		sendSock.socketReady = false;
		sendSock.host = host;
		sendSock.port = sendPort;

		try {                
			sendSock.socket = new TcpClient(sendSock.host, sendSock.port);
			sendSock.stream = sendSock.socket.GetStream();
			sendSock.writer = new StreamWriter(sendSock.stream);
			sendSock.socketReady = true;
		}
		catch (Exception e) {
			Debug.Log("Socket error:" + e);
			Application.Quit();
		}
	}

	void Start() {
		sendSock = new Socket ();
		host = "10.0.0.1";
		sendPort = 50001;
		setupSendSocket ();
	}

	public void writeToSocket(String s)
	{
		sendSock.writer.Write (s);
		sendSock.writer.Flush ();
	}
}

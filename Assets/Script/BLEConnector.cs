using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp;
using System.Threading;
public class BLEConnector : MonoBehaviour {


	static BLEConnector instance;
	private string[] sr =new string[]{"COM6","COM4","\\\\.\\COM11","\\\\.\\COM14","COM8"};
	public Connector[] myBLE=new Connector[5];
	public int max_sr = 2;
	private  Thread connectThread;
	public  bool connectflag=false;



	public bool GetConnectState(int i)
	{
		return myBLE [i].isConnect;
	}
	public int GetBLEpro(int i)
	{
		return myBLE [i].processed;
	}
	public float[] GetBLEeuler(int i)
	{
		return myBLE [i].euler;
	}
	public void SetConFlag(bool b)
	{
		connectflag = b;
	}
	public void StartIMU()
	{
		connectThread = new Thread (Connect);
		connectThread.IsBackground = true;
		connectflag = true;
		connectThread.Start ();
	}
	private void Connect()
	{
		
		int i = 0;

		while (connectflag) {
			if (!myBLE [i].isConnect) 
				myBLE [i].SetUp (sr [i], 921600, i);

			i = (i + 1) % max_sr;

		}
	}

	public void CloseIMU()
	{
		connectflag = false;
		for (int i = 0; i < max_sr; i++) {
			myBLE [i].close ();
		}
	}
	void Awake()
	{
		instance = this;
		DontDestroyOnLoad (this);
		for(int i=0;i<5;i++){
			myBLE [i] = new Connector ();
		}
	}
	void Start()
	{
		max_sr = 2;		

	}
	// Update is called once per frame
	void Update () {
		
	}


}

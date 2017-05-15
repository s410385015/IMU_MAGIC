using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AssemblyCSharp;
using System.Threading;
using UnityEngine.SceneManagement;
public class Magic : MonoBehaviour {


	public Text debug_log1;
	public Text debug_log2;
	BLEConnector BLE;
	public Button myB;
	private GameObject[] cross=new GameObject[4];
	public Text myText;

	private Sprite[] sr_image=new Sprite[4];

	private bool[] setImage=new bool[4];
	//private string[] sr =new string[]{"COM6","COM8","\\\\.\\COM11","\\\\.\\COM14","COM6"};
	private bool[] on_sr=new bool[4];
	private enum statue{Load,Connecting,Calibration,Ready,Switch};
	private statue mode;


	void Awake()
	{

		BLE = FindObjectOfType<BLEConnector> ();
		Debug.Log (BLE);

	}
	// Use this for initialization
	void Start () {

		BLE = FindObjectOfType<BLEConnector> ();
		Debug.Log ("sr:" + BLE.max_sr);
		sr_image [0] = Resources.Load<Sprite>("ccr");
		sr_image [1] = Resources.Load<Sprite>("ccb");
		sr_image [2] = Resources.Load<Sprite>("ccg");
		sr_image [3] = Resources.Load<Sprite>("ccyy");

		mode = statue.Load;

		myB.GetComponentInChildren<Text> ().text = "Click to Connect";
		myText.text = "";
	


		for (int i = 0; i < 4; i++) {
			on_sr [i] = false;
			setImage [i] = false;
		}
		myB.onClick.AddListener (myB_click);

	
	

		for (int i = 0; i < 4; i++) {
			string temp = "cross" + i;
			cross [i] = GameObject.Find (temp);
			cross[i].SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if (mode == statue.Connecting) {
			if(myText.text=="")
				InvokeRepeating("ShowText",0f,0.5f);
			
			int t = 0;
			for(int i=0;i<BLE.max_sr;i++)
			{
				if (BLE.GetConnectState(i) && !setImage [i]) {
					ConnectSucc (i);
					t++;
				}
			}
			Debug.Log ("Con:"+t+" "+BLE.max_sr);
			if (t == BLE.max_sr)
				myB_click ();
		} else if (mode == statue.Calibration) {

			int n = 0;
			for (int i = 0; i < BLE.max_sr; i++) {
				if (BLE.GetBLEpro (i) >= Gobal.CAL / 4) {
					n++;

				}
			}
			Debug.Log ("Cal:"+n+" "+BLE.max_sr);
			if (n == BLE.max_sr)
				mode = statue.Ready;
			if(myText.text=="")
				InvokeRepeating("ShowText2",0f,0.5f);
			debug_log1.text = BLE.GetBLEpro (0).ToString();
		}
		else if (mode == statue.Ready) {

			float[] temp = new float[3];
			temp = BLE.GetBLEeuler (0);
			debug_log1.text = temp [0] + " " + temp [1] + " " + temp [2]; 

			mode = statue.Switch;
			CancelInvoke("ShowText2");
			DontDestroyOnLoad (BLE);
			SceneManager.LoadScene ("GameVer1");

		}

		if (mode!=statue.Switch&&Input.GetKeyDown ("escape")) {
			Debug.Log ("close");
			BLE.CloseIMU ();
		}
		//myB.GetComponentInChildren<Text> ().text=str;
	}


	public void ShowText()
	{
		if (myText.text == "" || myText.text == "Connecting...")
			myText.text = "Connecting.";
		else if(myText.text=="Connecting.")
			myText.text="Connecting..";
		else if(myText.text=="Connecting..")
			myText.text="Connecting...";
	}
	public void ShowText2()
	{
		if (myText.text == "" || myText.text == "Calibrating...")
			myText.text = "Calibrating.";
		else if(myText.text=="Calibrating.")
			myText.text="Calibrating..";
		else if(myText.text=="Calibrating..")
			myText.text="Calibrating...";
	}
	public void myB_click()
	{
		if (mode == statue.Load) {
			myB.GetComponentInChildren<Text> ().text = "STOP";
			for (int i = 0; i < 4; i++) {
				cross [i].SetActive (true);
			}
			BLE.StartIMU ();
			mode=statue.Connecting;
		} else if (mode == statue.Connecting) {

			//myB.gameObject.SetActive (false);
			CancelInvoke("ShowText");
			myText.text="";
			mode=statue.Calibration;
			BLE.SetConFlag (false);
		} 
	}


	public void ConnectSucc(int n)
	{
		print (n + " " + sr_image [n]);
		cross [n].GetComponent<SpriteRenderer> ().sprite = sr_image [n];
	}


}

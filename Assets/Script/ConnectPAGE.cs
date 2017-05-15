using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ConnectPAGE : MonoBehaviour {

	public Button myB;
	private GameObject[] cross=new GameObject[4];
	private Sprite[] sr_image=new Sprite[4];

	// Use this for initialization
	void Start () {

		sr_image [0] = Resources.Load<Sprite>("ccr");
		sr_image [1] = Resources.Load<Sprite>("ccb");
		sr_image [2] = Resources.Load<Sprite>("ccg");
		sr_image [3] = Resources.Load<Sprite>("ccyy");

		for (int i = 0; i < 4; i++) {
			string temp = "cross" + i;
			cross [i] = GameObject.Find (temp);
			cross[i].SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

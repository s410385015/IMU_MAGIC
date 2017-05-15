using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMove : MonoBehaviour {
	public GameObject expO;
	public float speed=0.01f;
	public float distance=5.7f;
	public float range_in=5.06f;
	public float range_out=5.46f;
	public float range_good=0.15f;
	public float x;
	public float y;
	public int mode = 1;
	public KeyCode[] key = { 0, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.DownArrow };
	private bool flag = false;
	// Use this for initialization
	void Start () {

	}
	public void ChangeMode(int n)
	{
		mode = n;
		switch (mode) {
		case 1:
			GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("紅音符");
			GetRate (1);
			break;
		case 2:
			GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("藍音符");
			GetRate (0);
			break;
		case 3:
			GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("紫音符");
			GetRate (0);
			x = -x;
			break;
		case 4:
			GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("綠音符");
			GetRate (1);
			y = -y;
			break;
		}
		flag = true;
	}
	// Update is called once per frame
	void Update () {
		if (flag) {
			float cur = gameObject.transform.position.x * gameObject.transform.position.x +
			           gameObject.transform.position.y * gameObject.transform.position.y;

			if (cur > distance * distance) {
				//Instantiate (expO, transform.position, transform.rotation);
				Destroy (gameObject);
				print ("fail");
			}

			Check (cur);
			gameObject.transform.position += new Vector3 (x, y, 0);
		}
	}


	void Check(float cur)
	{
		/*
		if (mode==Magic.Instance.handmode) {
			if (cur > range_in * range_in && cur < (range_in + range_good) * (range_in + range_good)) {
				Instantiate (expO, transform.position, transform.rotation);
				Destroy (gameObject);
				print ("good");

			} else if (cur > (range_in + range_good) * (range_in + range_good) && cur < (range_out - range_good) * (range_out + range_good)) {
				Instantiate (expO, transform.position, transform.rotation);
				Destroy (gameObject);
				print ("great");
			} else if (cur > (range_out - range_good) * (range_out - range_good) && cur < (range_out) * (range_out)) {
				Instantiate (expO, transform.position, transform.rotation);
				Destroy (gameObject);
				print ("soso");
			}
		}
		*/
	}

	void GetRate(int n)
	{
		if (n == 0) {
			//x = Random.Range (0.0075f, 0.01f);
			x=Random.Range (0.8f*speed, speed);
			y = Mathf.Sqrt(speed * speed - x * x);
			y = Random.Range (1, 10) % 2 == 1 ? y : -y;
		} 
		else {
			x = Random.Range (0, 0.7f*speed);
			y = Mathf.Sqrt(speed * speed - x * x);
			x = Random.Range(1, 10) % 2 == 1 ? x : -x; 
		}
	}
}

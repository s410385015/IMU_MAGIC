using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMove : MonoBehaviour {

	public GameObject expO;

	//private bool expflag=false;
	public float speed=0.01f;
	public float distance=3.20f;
	public float range_out=4.10f;
	public float range_pefect=0.10f;
	public float range_good=0.25f;
	public float x;
	public float y;
	public int mode = 1;
	public int id=0;
	public KeyCode[] key = { 0, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.DownArrow };
	private const float Yoffset = -0.5f;
	private bool flag = false;
	//private float time=0;
	//private int DestroyCount = 0;
	// Use this for initialization
	void Start () {
		
	}
	public void ChangeMode(int n,int i,GameObject g)
	{
		
		expO = g;
		mode = n;
		id = i;
		switch (mode) {
		case 1://向上
			GetRate (1);
			if(x<0)gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))-90f));
			else gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))-135f));
			break;
		case 2://向右
			GetRate (0);
			if(y<0)gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))-225f));
			else gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))-180f));
			break;
		case 3:	//向左		
			GetRate (0);
			x = -x;
			if(y<0)gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))));
			else gameObject.transform.Rotate (new Vector3 (0f, 0f, -Mathf.Abs(Mathf.Atan2(x,y))-45f));
			break;
		case 4:
			GetRate (1);
			y = -y;
			break;
		}
		flag = true;
	}
	// Update is called once per frame
	void Update () {
		if (MainGame.Instance.mode == MainGame.state.Stop) {
			gameObject.GetComponent<Animator> ().enabled = false;

		}
		if (flag&&MainGame.Instance.mode!=MainGame.state.Stop) {
			if(!gameObject.GetComponent<Animator> ().enabled ) {
				gameObject.GetComponent<Animator> ().enabled = true;
			
			}

			float cur = gameObject.transform.position.x * gameObject.transform.position.x +
				(gameObject.transform.position.y-Yoffset) * (gameObject.transform.position.y-Yoffset);

			if (cur > range_out* range_out) {
				//Instantiate (expO, transform.position, transform.rotation);
				MainGame.Instance.ComboShow (0);
				Destroy (gameObject);
			}
			//MainGame.Instance.DebugLog.text = Mathf.Sqrt (cur).ToString();
			Check (cur);
			gameObject.transform.position += new Vector3 (x, y, 0);

		}
		/*
		if (!flag && expflag) {
			time += Time.deltaTime;
			if (time > 0.2f) {
				Vector3 v = transform.position;
				v.x =v.x+x*35;
				v.y = v.y + y * 35;
				Instantiate (expO, v, transform.rotation);
				time = 0;
				DestroyCount++;
			}
			if (DestroyCount == 3) {
				Destroy (gameObject);
			}
		}
		*/
	}

	//0-fail 1-good 2-perfect
	void Check(float cur)
	{
		cur = Mathf.Sqrt (cur);
		//Debug.Log (MainGame.Instance.IMUID[id]+" "+mode);
		if (mode==MainGame.Instance.IMUID[id]) {
			Vector3 v = transform.position;
			v.x =v.x+x*35;
			v.y = v.y + y * 35;
			if (Mathf.Abs (cur - distance) <= range_pefect) {
				Instantiate (expO, v, transform.rotation);
				MainGame.Instance.ComboShow (2);		
				Destroy (gameObject);
			} else if (Mathf.Abs (cur - distance) > range_pefect && Mathf.Abs (cur - distance) <= range_good) {
				Instantiate (expO, v, transform.rotation);
				MainGame.Instance.ComboShow (1);
				Destroy (gameObject);
			}
			
		}

	}

	void GetRate(int n)
	{
		if (n == 0) {
			//x = Random.Range (0.0075f, 0.01f);
			x = Random.Range (0.8f * speed, speed);
			y = Mathf.Sqrt (speed * speed - x * x);
			y = Random.Range (1, 11) % 2 == 1 ? y : -y;
		} else {
			x = Random.Range (0, 0.7f * speed);
			y = Mathf.Sqrt (speed * speed - x * x);
			x = Random.Range (1, 11) % 2 == 1 ? x : -x; 
		}
	}
}

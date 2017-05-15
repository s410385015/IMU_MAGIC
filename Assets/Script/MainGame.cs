using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainGame : MonoBehaviour {
	public enum state{Load,Connect,Ready,Play,Pause,Over,Stop,ResultLoad,Result};
	public static MainGame Instance;
	public GameObject Ready;
	public GameObject Ready_;
	public GameObject Combo_Perfect;
	public GameObject Combo_Good;
	public GameObject Combo_Fail;
	public GameObject Combo_Num;
	public GameObject c;
	public GameObject inner;
	public GameObject Arrow;
	public GameObject c1;
	public GameObject c2;
	public GameObject c3;
	public GameObject c4;
	public GameObject s0;
	public GameObject s1;
	public GameObject s2;
	public GameObject s3;
	public GameObject s4;
	public GameObject crystal;
	public GameObject[] exp;
	public GameObject bar;
	public GameObject[] MagicDir;
	public GameObject PausePage;
	public GameObject PageSwitcher;
	public GameObject Result;
	public Text DebugLog;
	public Text DebugFps;
	public Text Score;
	private int fps = 0;
	private int index=1;
	private float fpsTime = 0;
	public int ReadyShine;
	public float time;
	public bool ReadyFlag;
	public state mode;
	public int[] IMUID;
	public AudioSource song;
	BLEConnector BLE;
	private GameBox gb;
	private bool GOflag=false;
	private float deg=0;
	private GameObject tempObj;
	private Number numSp=new Number();
	private const float RingSize = 0.55f;
	private bool Progress_Shine=false;
	public GameObject ProgressBlur;

	// Use this for initialization
	void Awake()
	{
		BLE = FindObjectOfType<BLEConnector> ();
		Debug.Log (BLE);
	}
	void Start () {
		
		numSp.LoadSp ();
		Debug.Log (numSp.GetW (0));	
		gb = new GameBox ();
		gb.SetDiff (index);
		song.Stop ();
		Instance = this;
		mode = state.Load;
		ReadyShine = 0;
		ReadyFlag = true;
		InvokeRepeating("ShowReady",0f,0.5f);
		c1.SetActive (false);
		c2.SetActive (false);
		c3.SetActive (false);
		c4.SetActive (false);
		crystal.SetActive (false);
		PausePage.SetActive (false);
		Result.SetActive (false);
		IMUID = new int[5];
		for (int i = 0; i < 5; i++)
			IMUID [i] = new int();
		
	}
	
	// Update is called once per frame
	void Update () {

			
			if(Time.time>fpsTime)
			{
				fpsTime += 1;
				DebugFps.text = "Fps:" + fps;
				fps = 0;
			}
			fps++;
			if(mode==state.Load){
				DebugLog.text = GetYDeg ().ToString ();
				time += Time.deltaTime;
				if (GOflag||Input.GetKeyDown ("escape")) {
					mode = state.Ready;
					
				}

				//手往下甩
				if(GetYDeg()<-35&&deg>35)
				{
					GOflag = true;	
				}
				if (GetYDeg () > 35) {
					deg = 40;
				}
				if (time > 1f) {
					deg = 0;
					time = 0f;
				}
			}
			
		if (mode == state.Ready) {
			
			if (Ready.transform.localScale.y > 0f) {
				Ready.transform.localScale -= new Vector3 (0f, 0.03f, 0f);
				Ready_.transform.localScale -= new Vector3 (0f, 0.03f, 0f);
			} else if (Ready.transform.localScale.y <= 0f && c.transform.localScale.x < RingSize) {
				
				Ready.SetActive (false);
				Ready_.SetActive (false);
				c.transform.localScale += new Vector3 (0.01f, 0.01f, 0f);
				c.transform.Rotate (new Vector3 (0f, 0f, 4.5f), Space.Self);
				Arrow.transform.localScale += new Vector3 (0.01f, 0.01f, 0);
				Arrow.transform.Rotate (new Vector3 (0f, 0f, -4.5f), Space.Self);
				CancelInvoke ("ShowReady");

			} else {
				inner.transform.localScale = new Vector3 (RingSize, RingSize, 0);
				crystal.SetActive (true);
				mode = state.Play;
				song.Play ();
				time = 0;
				InvokeRepeating ("ProgressShine", 0f, 0.2f);
			}
		} else if (mode == state.Play) {

			//Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			//DebugLog.text = ray.ToString ();
			Arrow.transform.Rotate (new Vector3 (0f, 0f, -0.1f), Space.Self);
			CheckDir (0);
			if (BLE != null)
				for (int i = 0; i < BLE.max_sr; i++) {
					CheckDir (i);
				}
			bar.transform.position += new Vector3 (0.005f, 0f, 0f);
			time += Time.deltaTime;
				
				


			if (time > 2f) {
				StartCoroutine (Generator ());
				time = 0f;
			}
		
			if (Input.GetKeyDown ("s")) {
				mode = state.Stop;
				song.Pause ();
				crystal.GetComponent<Animator> ().enabled = false;
				PausePage.SetActive (true);
				BLE.CloseIMU ();
			}

		} else if (mode == state.Stop) {
			if (Input.GetKeyDown ("s")) {
				mode = state.Play;
				crystal.GetComponent<Animator> ().enabled = true;
				song.Play ();
			}
		} else if (mode == state.ResultLoad) {
			ResultLoad ();
		} else if (mode == state.Result) {
			Debug.Log ("LoadResult");



		}
	}
	public void ResultShow()
	{
		Result.SetActive (true);
	}
	public void ResultSet()
	{
		ResultClass r = Result.GetComponent<ResultClass> ();
		r.LoadResult = true;
	}
	public void ResultLoad()
	{
		ResultClass r = Result.GetComponent<ResultClass> ();
		r.SetInfo("Bad Apple",3,gb.Perfect,gb.Good,gb.Fail,gb.HighestCombo,gb.GetPer(1),gb.GetPer(0),!gb.isFail());
		c.transform.localScale -= new Vector3 (0.01f, 0.01f, 0f);
		c.transform.Rotate (new Vector3 (0f, 0f, 4.5f), Space.Self);
		Arrow.transform.localScale -= new Vector3 (0.01f, 0.01f, 0);
		Arrow.transform.Rotate (new Vector3 (0f, 0f, -4.5f), Space.Self);
		if (c.transform.localScale.x < 0.01) {
			mode = state.Result;
			Instantiate (PageSwitcher, transform.position, transform.rotation);
		}

	}

	public void GameStop()
	{
		mode = state.Stop;
		song.Pause ();
		crystal.GetComponent<Animator> ().enabled = false;
		PausePage.SetActive (true);
	}
	public void GamePlay()
	{
		mode = state.Play;
		crystal.GetComponent<Animator> ().enabled = true;
		song.Play();
		PausePage.SetActive (false);
		Debug.Log ("Play");
	}
	public void GameRetry()
	{
		PausePage.SetActive (false);
	}
	public void GameExit()
	{
		PausePage.SetActive (false);
		inner.SetActive (false);
		crystal.SetActive (false);
		mode = state.ResultLoad;
	}

	public IEnumerator Generator()
	{
		gb.GenerateNote ();
		Vector3 pos = new Vector3 (0, -0.5f, 0);
		int handID = gb.noteID;
		int modeDir = gb.noteAngle;
		GameObject note;
		switch (handID) {
		case 1:
			note = Instantiate (s1, pos, transform.rotation);

			break;
		case 2:
			note = Instantiate (s2, pos, transform.rotation);

			break;
		case 3:
			note = Instantiate (s3, pos, transform.rotation);
		
			break;
		case 4:
			note = Instantiate (s4, pos, transform.rotation);
			break;
		default:
			note=Instantiate (s0, pos, transform.rotation);
			break;
		}
		note.AddComponent (typeof(StarMove));
		StarMove spt=(StarMove)note.GetComponent (typeof(StarMove));
		spt.ChangeMode (modeDir,handID,exp[handID-1]);
		yield return null;
	}
	public IEnumerator NumberShow()
	{
		int len = gb.Combo.ToString ().Length;
		int n = gb.Combo;
		Vector3 pos = new Vector3 ((len-1)*0.25f, -1.5f, 0);
		while (len!=0) {
			GameObject Obj=Instantiate (Combo_Num, pos, transform.rotation);
			pos-=new Vector3(0.5f,0,0);
			TextCombo tc = (TextCombo)Obj.GetComponent (typeof(TextCombo));
			tc.SetSprite (n % 10, numSp.GetW (n % 10), numSp.GetY (n % 10));
			n /= 10;
			len--;
		}
		yield return null;
	}

	public void ComboShow(int n)
	{
		if (gb.Combo == 0)
			ComboDestroy ();
		gb.Hit (n);
		Vector3 pos = new Vector3 (0, -2.5f, 0);
		//Vector3 pos = new Vector3 (5.1f, -3.5f, 0);
		if (n == 1) {
			ShowPersent();
			//Score.text = (gb.Suc * 0.6).ToString ()+"%";
			//pos += new Vector3 (0.4f, 0, 0);
			StartCoroutine(NumberShow());
			tempObj = Instantiate (Combo_Good, pos, transform.rotation);
		} else if (n == 2) {
			ShowPersent();
			//Score.text = (gb.Suc * 0.6).ToString ()+"%";
			StartCoroutine(NumberShow());
			tempObj = Instantiate (Combo_Perfect, pos, transform.rotation);
		} else if (n == 0) {
			//pos += new Vector3 (0.5f, 0, 0);

			tempObj = Instantiate (Combo_Fail, pos, transform.rotation);
			tempObj.GetComponent<Combo> ().SetDestoryTime (1f);
		}

	}
	public void ShowPersent()
	{
		float s = (gb.Suc - 1) * 0.6f;
		float f = (gb.Suc ) * 0.6f;
		Score.text=Mathf.Lerp(s,f,Time.time).ToString()+"%";
	}

	public void ComboDestroy()
	{
		if (tempObj != null)
			Destroy (tempObj);
	}

	public void ProgressShine()
	{

		if (Progress_Shine) {
			DebugLog.text="+";
			ProgressBlur.GetComponent<SpriteRenderer> ().color += new Color (0f, 0f, 0f, 0.1f);
			if (ProgressBlur.GetComponent<SpriteRenderer> ().color.a > 0.9f)
				Progress_Shine = false;
		} else{
			DebugLog.text="-";
			ProgressBlur.GetComponent<SpriteRenderer> ().color -= new Color (0f, 0f, 0f, 0.1f);
			if (ProgressBlur.GetComponent<SpriteRenderer> ().color.a < 0.1f)
				Progress_Shine = true;
		}
	}
	public void ShowReady()
	{
		if (ReadyShine==0) {
			ReadyShine = 1;
			Ready.SetActive (true);
			Ready_.SetActive (false);
		} 
		else if(ReadyShine==1){
			ReadyShine = 2;
			Ready.SetActive (false);
			Ready_.SetActive (true);
		}
		else{
			ReadyShine = 0;
			Ready.SetActive (false);
			Ready_.SetActive (true);
		}
	}

	public float GetYDeg()
	{
		if (BLE != null) {
			float[] e = new float[3];
		
			e = BLE.GetBLEeuler (0);
			return e [1];
		}
		return 0;
	}

	public void CheckDir(int n)
	{
		float[] e = new float[3];
		float deg_x=0;
		float deg_y=0;
		float deg_z = 0;
		if (BLE != null) {
			e = BLE.GetBLEeuler (n);
			deg_x = e [0];
			deg_y = e [1];
			deg_z = e [2];
		}
	
		int keyin=0;

		if (Input.GetKey (KeyCode.UpArrow))
			keyin = 1;
		else if (Input.GetKey (KeyCode.LeftArrow))
			keyin = 2;
		else if (Input.GetKey (KeyCode.RightArrow))
			keyin = 3;


		//if (n == 0) {
			//DebugLog.text = deg_x + " " + deg_y + " " + deg_z;
		//}

		int dir = gb.CheckDir (n, deg_x, deg_y, deg_z);
			//往左
		if ((dir==3)||keyin==2) {
			MagicDir[n].SetActive (true);
			Quaternion quate = Quaternion.identity;
			quate.eulerAngles = new Vector3 (0, 0, 90f);
			MagicDir[n].transform.rotation = quate;
			IMUID [n+1] = 3;
		
		}

		//往右
		else if (dir==2||keyin==3) {
			MagicDir[n].SetActive (true);
			Quaternion quate = Quaternion.identity;
			quate.eulerAngles = new Vector3 (0, 0, -90f);
			MagicDir[n].transform.rotation = quate;
			IMUID [n+1] = 2;


		}


		else if (dir==1||keyin==1) {
			MagicDir[n].SetActive (true);
			Quaternion quate = Quaternion.identity;
			quate.eulerAngles = new Vector3 (0, 0, 0f);
			MagicDir[n].transform.rotation = quate;
			IMUID [n+1] = 1;
		} 
		else {
			MagicDir[n].SetActive (false);
			IMUID [n+1] = 0;
		}

		/*
		if (deg >= 225 && deg < 315) {
			MagicDir[n].SetActive (false);
			Quaternion quate = Quaternion.identity;
			quate.eulerAngles = new Vector3 (0, 0, -225);
			MagicDir[n].transform.rotation = quate;
			handmode = 3;
		}
		*/
	}


}

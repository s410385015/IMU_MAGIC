using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultClass : MonoBehaviour {
	private enum state{result,perfect,good,fail,combo,persent,none,done};
	private state st;
	//0-name
	//1-result
	//2-perfect
	//4-good
	//6-fail
	//8-combo
	//10-persent
	//11-complete
	public Text[] text;
	public int[] Count;
	public float Count_f;
	public float Count_b;
	public float per;
	public int Level;
	public GameObject[] crown;
	public GameObject star;
	private string[] str=new string[12];
	public int flag = 0;
	public bool starflag=true;
	public int starCount = 0;
	public bool LoadResult = false;
	// Use this for initialization
	void Start () {
		st = state.perfect;
		str[0]="null";
		str[1]="RESULT";
		str[2]="Perfect";
		str[3]="0";
		str[4]="Good";
		str[5]="0";
		str[6]="Fail";
		str[7]="0";
		str[8] = "Combo";
		str[9]="0";
		str[10]="0%";
		str[11]="Complete";
		crown[0].SetActive (false);
		crown[1].SetActive (false);
		crown[2].SetActive (false);
		Debug.Log (Count.Length.ToString ());
	}
	
	// Update is called once per frame
	void Update () {
		if (LoadResult) {
			if (starflag && starCount < Level) {
				starflag = false;
				Vector3 v = new Vector3 (-2.7f + starCount * 0.7f, 4.65f, 0f);
				GameObject temp = Instantiate (star, v, gameObject.transform.rotation);
				StartCoroutine (FadeIn (temp));
				starCount++;
			}

			if (Input.GetMouseButtonDown (0)) {
				st = state.done;

			}

			if (st == state.perfect) {
				StartCoroutine (ShowText (2));
				st = state.good;
			} else if (st == state.good) {
				StartCoroutine (ShowText (4));
				st = state.fail;
			} else if (st == state.fail) {
				StartCoroutine (ShowText (6));
				st = state.combo;
			} else if (st == state.combo) {
				StartCoroutine (ShowText (8));
				st = state.persent;
			} 

			if (st == state.persent && flag == 4) {
				StartCoroutine (ShowNumf ());
				st = state.result;
			}

			if (st == state.result && flag == 5) {
				StartCoroutine (FadeIn (text [11]));
				if (Count_f >= 90.0f)
					crown [0].SetActive (true);
				else if (Count_f >= 75f)
					crown [1].SetActive (true);
				else if (Count_f >= 60f) {
					Debug.Log ("Broze");
					crown [2].SetActive (true);
				}
				flag++;
			}

			if (st == state.done) {
				Debug.Log ("Y");

				for (int i = 0; i < 12; i++) {
					if (i == 10)
						text [i].text = str [i] + "%";
					else
						text [i].text = str [i];
				}
				if (Count_f >= 90.0f)
					crown [0].SetActive (true);
				else if (Count_f >= 75f)
					crown [1].SetActive (true);
				else if (Count_f >= 60f) {
					Debug.Log ("Broze");
					crown [2].SetActive (true);
				}
				if (flag == 5)
					st = state.none;
			}
		}
	}

	public void SetInfo(string n,int l,int p,int g,int f,int c,float per,float per_base,bool r)
	{
		Level = l;
		str [0] = n;
		str[3]=p.ToString();
		str[5]=g.ToString();
		str[7]=f.ToString();
		str[9]=c.ToString();
		str [10] = per.ToString ();
		str [11] = r ? "Complete" : "Fail";

		text [0].text = str [0].ToString ();
		Count = new int[4];
		Count[0]= p;
		Count[1] = g;
		Count[2] = f;
		Count[3] = c;
		Count_f = per;
		Count_b = per_base;
	}

	public IEnumerator ShowText(int n)
	{
		int t = str [n].Length;
		while (t--!=0) {
			text [n].text = str [n].Substring (0, str [n].Length - t);
			if (st == state.done)
				break;
			yield return new WaitForSeconds (0.1f);
		}

		if(n!=0)
			StartCoroutine (ShowNum (n/2-1));
	}

	public IEnumerator ShowNum(int n)
	{	
		int t = 0;
		while (t <= Count [n]) {
			text [(n + 1) * 2+1].text = t.ToString (); 
			t++;
			if (st == state.done)
				break;
			yield return new WaitForSeconds (0.01f);
		}
		flag++;
	}

	public IEnumerator ShowNumf()
	{	
		float t = Count_b;
		while (t <= Count_f) {
			text [10].text = t.ToString ("0.00")+"%"; 
			t += 0.5f;
			if (st == state.done)
				break;
			yield return null;
		}
		flag++;
	}
	public IEnumerator FadeIn(Text i){
		i.text = str [11];
		i.color = new Color (i.color.r, i.color.g, i.color.b, 0);
		while (i.color.a < 1.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / 0.2f));
			if (st == state.done)
				break;
			yield return null;
		}
	}

	public IEnumerator FadeIn(GameObject obj){
		SpriteRenderer i = obj.GetComponent<SpriteRenderer> ();
		i.color = new Color (i.color.r, i.color.g, i.color.b, 0);
		while (i.color.a < 1.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / 0.2f));
			yield return null;
		}
		starflag = true;
	}
}

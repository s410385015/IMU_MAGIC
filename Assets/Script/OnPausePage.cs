using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPausePage : MonoBehaviour {

	public enum B_ID{None,Pause,Play,Retry,Exit};
	public B_ID mode;
	private float t;
	// Use this for initialization
	void Start () {
		t = transform.localScale.x;
		//mode = B_ID.None;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown()
	{
		switch (mode) {
		case B_ID.Play:
			Debug.Log ("?");
			MainGame.Instance.GamePlay ();
			break;
		case B_ID.Pause:
			Debug.Log ("!");
			MainGame.Instance.GameStop ();
			break;
		case B_ID.Retry:
			Debug.Log ("Q"+gameObject.name);
			MainGame.Instance.GameRetry ();
			break;
		case B_ID.Exit:
			Debug.Log ("E");
			MainGame.Instance.GameExit ();
			break;
		}
	}

	void OnMouseEnter()
	{
		if(mode!=B_ID.Pause)
			transform.localScale = new Vector3 (t+0.2f, t+0.2f, transform.localScale.z);
	}

	void OnMouseExit()
	{
		
		transform.localScale = new Vector3 (t, t, transform.localScale.z);
	}

}


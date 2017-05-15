using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour {
	public enum ComboState{Init,Shine,Destory};
	public float time;
	public GameObject word;
	public GameObject word_shine;
	private float speed;
	private ComboState mode;
	private float normalSize = 0.2f;
	private float BigSize = 0.3f;
	private float DestoryTime=1.5f;
	// Use this for initialization
	void Start () {
		time = 0;
		speed = 0.002f;
		mode = ComboState.Init;
		word.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
		word_shine.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (word.transform.localScale.x < BigSize && mode == ComboState.Init) {
			word.transform.localScale += new Vector3 (speed, speed, 0);
			word_shine.transform.localScale += new Vector3 (speed, speed, 0);
			speed += 0.002f;
			if (word.transform.localScale.x > BigSize) {
				mode = ComboState.Shine;
			}	
		} else if (mode == ComboState.Shine) {
			word.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
			word_shine.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
		} else if (mode == ComboState.Destory) {
			Destroy (gameObject);
		}
		if (time > DestoryTime)
			Destroy (gameObject);
	}
	public void SetDestoryTime(float t)
	{
		DestoryTime = t;
	}
}

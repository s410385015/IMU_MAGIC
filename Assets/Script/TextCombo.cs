using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextCombo : MonoBehaviour {

	public enum ComboStatue{Init,Shine,Destory};
	public float time;
	public GameObject word;
	public GameObject word_shine;
	private float speed;
	private ComboStatue mode;
	private float normalSize = 0.2f;
	private float BigSize = 0.3f;
	private float DestoryTime=1.5f;
	private bool flag=false;
	// Use this for initialization
	void Start () {
		time = 0;
		speed = 0.002f;
		mode = ComboStatue.Init;
		word.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
		word_shine.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
	}
	// Update is called once per frame
	void Update () {
		if (flag) {
			time += Time.deltaTime;
			if (word.transform.localScale.x < BigSize && mode == ComboStatue.Init) {
				word.transform.localScale += new Vector3 (speed, speed, 0);
				word_shine.transform.localScale += new Vector3 (speed, speed, 0);
				speed += 0.002f;
				if (word.transform.localScale.x > BigSize) {
					mode = ComboStatue.Shine;
				}	
			} else if (mode == ComboStatue.Shine) {
				word.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
				word_shine.transform.localScale = new Vector3 (normalSize, normalSize, 0f);
			} else if (mode == ComboStatue.Destory) {
				Destroy (gameObject);
			}
			if (time > DestoryTime)
				Destroy (gameObject);
		}
	}
	public void SetSprite(int n,Sprite sp,Sprite sp_s)
	{
		word.GetComponent<SpriteRenderer> ().sprite = sp;
		word_shine.GetComponent<SpriteRenderer> ().sprite = sp_s;
		flag = true;
	}
}
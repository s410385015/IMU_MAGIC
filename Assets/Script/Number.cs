using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Number : MonoBehaviour {
	private Sprite[] number_w=new Sprite[10];
	private Sprite[] number_y=new Sprite[10];
	private Sprite[] number_b=new Sprite[10];
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void LoadSp()
	{
		for(int i=0;i<10;i++)
			number_w [i] = Resources.Load<Sprite>(i+"-w");
		for(int i=0;i<10;i++)
			number_y [i] = Resources.Load<Sprite>(i+"-y");
		for(int i=0;i<10;i++)
			number_b [i] = Resources.Load<Sprite>(i.ToString());
	}
	public Sprite GetW(int n)
	{
		return number_w [n];
	}
	public Sprite GetY(int n)
	{
		return number_y [n];
	}
	public Sprite GetB(int n)
	{
		return number_b [n];
	}

}

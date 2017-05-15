using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageSwitcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Load()
	{
		MainGame.Instance.ResultShow ();
	}
	void End()
	{
		MainGame.Instance.ResultSet ();
		Destroy (gameObject);
	}
}

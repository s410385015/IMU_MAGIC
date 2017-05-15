using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class videoPlayer : MonoBehaviour {
	public MovieTexture mt;
	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().sortingLayerName="Back1";
		GetComponent<RawImage> ().texture= mt as MovieTexture;
		mt.loop = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!mt.isPlaying)
			mt.Play ();
		Debug.Log (mt.isPlaying);
	}
}

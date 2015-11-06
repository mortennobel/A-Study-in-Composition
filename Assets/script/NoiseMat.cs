using UnityEngine;
using System.Collections;

public class NoiseMat : MonoBehaviour {

	Material mat;
	public float scale = 0.5f;
	public Vector4 windDir = new Vector4 (1, 0, 0, 0);
	// Use this for initialization
	void Start () {
		//mat = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		//mat.SetVector("_Wind", windDir*Runevision.Structures.SimplexNoise.Noise(new Vector3(Time.time,transform.position.x,transform.position.y))*scale);
	}
}

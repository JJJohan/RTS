using UnityEngine;
using System.Collections;

public class Clouds : MonoBehaviour {
	
	public float xVel;
	public float yVel;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{		
		transform.position += new Vector3(xVel * Time.deltaTime, 0.0f, yVel * Time.deltaTime);
		transform.Rotate(new Vector3(0.0f, 0.1f * Time.deltaTime, 0.0f));
	}
}

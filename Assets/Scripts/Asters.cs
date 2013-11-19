using UnityEngine;
using System.Collections;

public class Asters : MonoBehaviour {
	
	public GameObject pref;
	
	private Transform player;
	void Start () {
		player=GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	public float dt=1,radius=1000,speed=100;private float last;
	public int max=100;
	void Update () {
		if(Time.time>last+dt) {
			last=Time.time;
			GameObject g=GameObject.Instantiate (pref) as GameObject;
			g.transform.position=Random.insideUnitSphere*radius+player.position;
			g.transform.rotation=Random.rotation;
			g.rigidbody.velocity=Random.insideUnitSphere*speed;
		}
	}
}

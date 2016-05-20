using UnityEngine;
using System.Collections;

/// <summary>
/// Simple script to make the Voxel asteroid move
/// </summary>
public class Asters : MonoBehaviour {

	/// <summary>
	/// Asteroid prefab
	/// </summary>
	public GameObject pref;
	
	private Transform player;
	void Start () {
		player=GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	public float dt=1;
	/// <summary>
	/// Radius around the player
	/// </summary>
	public float radius=1000;
	/// <summary>
	/// Speed of asteroid
	/// </summary>
	public float speed=100;
	private float last;
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

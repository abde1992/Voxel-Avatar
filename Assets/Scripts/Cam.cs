using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {


	public Vector2 speed = new Vector2(135.0f, 135.0f);
	public float yMinLimit = -90,yMaxLimit = 90;
	
	public float normalDistance=5;
	
	private float x = 0.0f,y = 0.0f;
	private float deltaTime;

	private VoxelTerrain v;
	private Transform player;

	void Start () {
		player=GameObject.FindGameObjectWithTag("Player").transform;

		v=GameObject.FindObjectOfType(typeof(VoxelTerrain)) as VoxelTerrain;

		Vector3 cp=Camera.main.transform.position;
		v.LoadMeshes(cp);
	}

	void LateUpdate () {

		deltaTime = Time.deltaTime;
		
		GetInput();
		
		CameraMovement();
	}
	
	void GetInput() {
		Vector2 a = speed;
		x += Input.GetAxis("Mouse X") * a.x * deltaTime;
		y -= Input.GetAxis("Mouse Y") * a.y * deltaTime;
		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}
	
	void CameraMovement() {
		
		Quaternion q=Quaternion.AngleAxis (-x,player.up)*Quaternion.AngleAxis (y,player.right);
		Vector3 dir=q*-player.forward*normalDistance;
		transform.position=player.position+dir;
		transform.rotation=Quaternion.LookRotation (player.position-transform.position,Vector3.up);
		
	}
	
	public static float ClampAngle(float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}

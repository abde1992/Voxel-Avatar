using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {
	public Transform player;
	
	public Vector2 speed = new Vector2(135.0f, 135.0f);
	public float yMinLimit = -90,yMaxLimit = 90;
	public float normalFOV = 60,zoomFOV = 30,lerpSpeed = 8.0f;
	
	public LayerMask hitLayer;
	
	public float normalDistance=5;
	
	public float x = 0.0f,y = 0.0f;
	private float deltaTime;
	private Transform aimTarget;
	
	private Animator anim;
	
	void Start () {
		if(player == null) { 
			Destroy(this);
			return;
		}
		
		anim=player.GetComponent<Animator>();
		aimTarget=Util.getAT (player.gameObject);
	}
	
	void LateUpdate () {
		if(!gameObject.activeInHierarchy||!player.gameObject.activeInHierarchy) return;
		
		deltaTime = Time.deltaTime;
		
		//GetInput();
		
		CameraMovement();
	}
	
	void GetInput() {
		Vector2 a = speed;
		x += Input.GetAxis("Mouse X") * a.x * deltaTime;
		y -= Input.GetAxis("Mouse Y") * a.y * deltaTime;
		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}
	
	void CameraMovement() {
		//camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, deltaTime * lerpSpeed);
		
		Quaternion q=Quaternion.AngleAxis (-x,player.up)*Quaternion.AngleAxis (y,player.right);
		Vector3 dir=q*-player.forward*normalDistance;
		transform.position=player.position+dir;
		transform.rotation=Quaternion.LookRotation (player.position-transform.position,Vector3.up);
		
		RaycastHit hit=new RaycastHit();
		if(Util.NonTriggerHit (transform.position,player,transform.forward,out hit,hitLayer)) {
			aimTarget.position=hit.point;
		} else aimTarget.position=transform.position+100*transform.forward;
	}
	
	public static float ClampAngle(float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}

using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {
	public Transform camTarget;
	private Transform target,aimTarget;
	
	public Vector2 speed = new Vector2(135.0f, 135.0f),aimSpeed = new Vector2(70.0f, 70.0f),maxSpeed = new Vector2(100.0f, 100.0f);
	public float yMinLimit = -90,yMaxLimit = 90;
	public float normalFOV = 60,zoomFOV = 30,lerpSpeed = 8.0f;
	
	private float deltaTime;
	private Transform camTransform;
	private Quaternion rotation;
	private Vector3 position,cPos;
	
	public LayerMask hitLayer;
	
	public Vector3 normalDirection=new Vector3(0,0,1),aimDirection,crouchDirection,aimCrouchDirection;
	public float normalHeight=3,crouchHeight,normalAimHeight,crouchAimHeight,maxHeight;
	public float normalDistance=5,crouchDistance,normalAimDistance,crouchAimDistance,minDistance;
	private float targetDistance,targetHeight;
	private Vector3 camDir;
	
	public float minShakeSpeed,maxShakeSpeed,minShake,maxShake = 2.0f,maxShakeDistance;
	public int minShakeTimes,maxShakeTimes;
	
	private bool shake;
	private float shakeSpeed = 2.0f,cShakePos,cShake,cShakeSpeed;
	private int shakeTimes = 8,cShakeTimes;
	
	public Transform radar,radarCamera;
	
	public float x = 0.0f,y = 0.0f;
	
	void Start () {
		cShakeTimes = 0;
		cShake = 0.0f;
		cShakeSpeed = shakeSpeed;
		
		target=new GameObject(camTarget.name+"Target").transform;
		target.parent = null;
		camTransform = transform;
		
		var angles = camTransform.eulerAngles;
		x = angles.y;
	    y = angles.x;
	    
	    targetDistance = normalDistance;
	    
	    cPos = camTarget.position + new Vector3(0, normalHeight, 0);
		
		//aimTarget=Util.getAT (camTarget.gameObject);
	}
	
	public bool aiming=false;
	void LateUpdate () {
		if(!camera.enabled) return;
		
		deltaTime = Time.deltaTime;
		
		GetInput();
		
		RotateSoldier();

		CameraMovement();
	}
	
	float ax,ay;bool aimTemp=false;
	void GetInput() {
		if(aiming&&!aimTemp) {ax=x;ay=y;}
		if(!aiming&&aimTemp) {x=ax;y=ay;}
		aimTemp=aiming;
		Vector2 a = aiming?aimSpeed:speed;
		if(turning) return;
		x += Mathf.Clamp(Input.GetAxis("Mouse X") * a.x, -maxSpeed.x, maxSpeed.x) * deltaTime;
		//x = -180+Mathf.Repeat(x+180,360);
		y -= Mathf.Clamp(Input.GetAxis("Mouse Y") * a.y, -maxSpeed.y, maxSpeed.y) * deltaTime;
		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}
	
	void RotateSoldier() {
		if(aiming) return;
		Vector3 f=transform.forward;
		f.y=0;f.Normalize ();
		if(f.magnitude==0) return;
		//camTarget.rotation=Quaternion.Slerp(camTarget.rotation,Quaternion.LookRotation(f,Vector3.up),0.1f);
	}
	
	bool turning=false;float tx,ty;
	public float turnTime=0.2f;
	IEnumerator Turn(float ttx,float tty) {
		if(turning) yield break;
		float dx=Mathf.Repeat (ttx-x,360);
		if(dx>180) {
			dx=-(360-dx);
		}
		ttx=x+dx;
		if(tty>180) tty=-(360-tty);
		tx=ttx;ty=tty;
		turning=true;
		float px=x,py=y;
		float t=Time.time,dt=turnTime;
		while(Time.time<t+dt) {
			x=Mathf.Lerp (px,tx,(Time.time-t)/dt);
			y=Mathf.Lerp (py,ty,(Time.time-t)/dt);
			yield return null;
		}
		turning=false;
	}
	
	void CameraMovement() {
		//camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, deltaTime * lerpSpeed);
		
		if(Input.GetAxis ("Horizontal")!=0) {
			Quaternion q=Quaternion.LookRotation (camTarget.forward,camTarget.up);
			StartCoroutine (Turn (q.eulerAngles.y,q.eulerAngles.x));
		}
		Vector3 f=target.forward,r=target.right;
		//f=transform.worldToLocalMatrix.MultiplyVector (f);
		//r=transform.worldToLocalMatrix.MultiplyVector (r);
		Vector3 dir=aiming?aimDirection:normalDirection;
		camDir = (dir.x * f) + (dir.z * r);
		//camDir=camTarget.localToWorldMatrix.MultiplyVector (camDir).normalized;
		//if(camDir.magnitude<Mathf.Epsilon) camDir=transform.forward;
		
		targetHeight = aiming?normalAimHeight:normalHeight;
		targetDistance = aiming?normalAimDistance:normalDistance;
		
		HandleCameraShake();
		cPos = camTarget.position + new Vector3(0, targetHeight, 0);
		
		RaycastHit hit=new RaycastHit();Vector3 nor=Vector3.zero;
		if(Physics.Raycast(cPos, camDir, out hit, targetDistance + 0.2f, hitLayer)) {
			float t = hit.distance - 0.1f;
			t -= minDistance;
			t /= (targetDistance - minDistance);

			targetHeight = Mathf.Lerp(maxHeight, targetHeight, Mathf.Clamp(t, 0.0f, 1.0f));
			cPos = camTarget.position + new Vector3(0, targetHeight, 0); 
		}
		
		if(Physics.Raycast(cPos, camDir, out hit, targetDistance + 0.2f, hitLayer)) {
			targetDistance = hit.distance - 0.1f;
			if(targetDistance<0) targetDistance=0;
			nor=hit.normal;
		}
		
		if(radar != null) {
			radar.position = cPos;
			radarCamera.rotation = Quaternion.Euler(90, x, 0);
		}
		
		Vector3 lookPoint = cPos;
		lookPoint += (target.right * Vector3.Dot(camDir * targetDistance, target.right));

		camTransform.position = cPos + (camDir * targetDistance)+nor*0.1f;
		camTransform.LookAt(lookPoint);
		
		target.position = cPos;
		if(!aiming) target.rotation = Quaternion.Euler(y, x, 0);
		
		/*if(!aiming) {
			if(Util.NonTriggerHit (camTransform.position,camTarget,camTransform.forward,out hit,hitLayer)) {
				aimTarget.position=hit.point;
			} else aimTarget.position=camTransform.position+100*camTransform.forward;
		} else {
			Quaternion q=Quaternion.Euler(y, x, 0);
			camDir=-(q*Vector3.forward*aimDirection.x+q*Vector3.right*aimDirection.z);
			camDir.Normalize ();
			Plane p=new Plane(-camDir,transform.position+camDir*10);
			float d;
			if(p.Raycast (new Ray(transform.position,camDir),out d)) {
				aimTarget.position=transform.position+d*camDir;
			}
		}*/
	}
	
	void HandleCameraShake() {
		if(shake) {
			cShake += cShakeSpeed * deltaTime;
			
			if(Mathf.Abs(cShake) > cShakePos) {
				cShakeSpeed *= -1.0f;
				cShakeTimes++;
				
				if(cShakeTimes >= shakeTimes)
					shake = false;
				
				if(cShake > 0.0f)
					cShake = maxShake;
				else
					cShake = -maxShake;
			}
			targetHeight += cShake;
		}
	}
	
	void StartShake(float distance) {
		float proximity = distance / maxShakeDistance;
		if(proximity > 1.0f) return;
		
		proximity = Mathf.Clamp(proximity, 0.0f, 1.0f);
		proximity = 1.0f - proximity;
		
		cShakeSpeed = Mathf.Lerp(minShakeSpeed, maxShakeSpeed, proximity);
		shakeTimes = (int) Mathf.Lerp(minShakeTimes, maxShakeTimes, proximity);
		cShakeTimes = 0;
		cShakePos = Mathf.Lerp(minShake, maxShake, proximity);
		
		shake = true;
	}
	
	public static float ClampAngle(float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}


using UnityEngine;
using System.Collections;

public class Fly : MonoBehaviour {

	void Start () {
	}
	
	public float maxLift=100,maxAcc=100,maxBankAcc=100;
	public float maxLiftTor=10,maxBankTorUp=10,maxBankTorFor=10;
	public float springLiftTor=1,springBankTor=1;
	public float lift,acc,bankAcc;
	public float liftTor,bankTorUp,bankTorFor;
	
	void FixedUpdate () {
		float ws=Input.GetAxis ("WS");
		float a=Input.GetAxis ("Vertical");
		float b=Input.GetAxis ("Horizontal");
		
		lift=ws*maxLift+Physics.gravity.magnitude;
		rigidbody.AddForce (transform.up*rigidbody.mass*lift,ForceMode.Force);
		liftTor=b!=0?0:ws*maxLiftTor;
		rigidbody.AddTorque (-transform.right*rigidbody.mass*liftTor);
		
		acc=a*maxAcc;
		rigidbody.AddForce (transform.forward*rigidbody.mass*acc,ForceMode.Force);
		
		bankAcc=b*maxBankAcc;
		rigidbody.AddForce (transform.right*rigidbody.mass*bankAcc,ForceMode.Force);
		bankTorUp=b*maxBankTorUp;
		bankTorFor=ws!=0?0:b*maxBankTorFor;
		rigidbody.AddTorque (transform.up*rigidbody.mass*bankTorUp);
		rigidbody.AddTorque (-transform.forward*rigidbody.mass*bankTorFor);
		
		rigidbody.AddTorque ((90-Vector3.Angle(transform.forward,Vector3.up))*transform.right*rigidbody.mass*springLiftTor);
		rigidbody.AddTorque (-(90-Vector3.Angle(transform.right,Vector3.up))*transform.forward*rigidbody.mass*springBankTor);
		
	}
}

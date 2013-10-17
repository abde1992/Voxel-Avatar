using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VRT))]
public class VRTEd : Editor {

	public override void OnInspectorGUI () {
	}
	
	void OnSceneGUI () {
		VRT v=target as VRT;
		Handles.BeginGUI ();
		if(GUI.Button (new Rect(10,10,100,100),"Make")) {
			v.GetComponent<MeshFilter>().sharedMesh=v.Make();
		}
		Handles.EndGUI ();
	}
}

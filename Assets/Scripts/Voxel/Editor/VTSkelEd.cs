using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VTSkel))]
public class VTSkelEd : Editor {

	public override void OnInspectorGUI () {
		VTSkel s=target as VTSkel;
		DrawDefaultInspector ();
		if(GUILayout.Button ("Make")) s.Make ();
		if(GUILayout.Button ("Tex")) {
			s.renderer.sharedMaterial.SetTexture ("CTN",VTMakeTex.getCTN ());
			s.renderer.sharedMaterial.SetTexture ("ECL",VTMakeTex.getECL ());
		}
	}
	
	void OnSceneGUI() {
		VTSkel s=target as VTSkel;
		int nx=Mathf.RoundToInt (s.size.x/s.dn);
		int ny=Mathf.RoundToInt (s.size.y/s.dn);
		int nz=Mathf.RoundToInt (s.size.z/s.dn);
		/*Handles.color=Color.red;
		for(int i=0,l=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {
					Handles.SphereCap (0,new Vector3(((float)i+0.5f)*s.dn,((float)j+0.5f)*s.dn,((float)k+0.5f)*s.dn),Quaternion.identity,0.1f);
				}
			}
		}*/
	}
}

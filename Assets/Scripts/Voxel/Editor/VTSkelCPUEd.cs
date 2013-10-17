using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VTSkelCPU))]
public class VTSkelCPUEd : Editor {
	
	float last;
	public override void OnInspectorGUI () {
		VTSkelCPU s=target as VTSkelCPU;
		DrawDefaultInspector ();
		if(GUILayout.Button ("Make")) {
			long t0=System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond;
			s.Make ();
			last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0);
		}
		if(GUILayout.Button ("Clear FC")) {
			s.filled_cells=null;
		}
		EditorGUILayout.LabelField ("Last Make Time:"+last);
		EditorGUILayout.LabelField((s.filled_cells!=null?s.filled_cells.GetLength (0)+"":""));
	}
	
	void OnSceneGUI() {
		VTSkelCPU s=target as VTSkelCPU;
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

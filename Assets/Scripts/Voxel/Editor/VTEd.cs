using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VoxelTerrain))]
public class VTEd : Editor {
	
	VoxelTerrain v;
	GameObject partCube;
	void OnEnable() {
		v=target as VoxelTerrain;
		if(v.transform.FindChild ("partCube")==null) {
			partCube=GameObject.CreatePrimitive(PrimitiveType.Cube);partCube.transform.parent=v.transform;
			partCube.name="partCube";
			partCube.renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath ("Assets/Materials/Highlight.mat",typeof(Material)) as Material;
		}
		partCube=v.transform.FindChild ("partCube").gameObject;
	}
	
	float last;
	public override void OnInspectorGUI () {
		v=target as VoxelTerrain;
		DrawDefaultInspector ();
		if(GUILayout.Button ("Init")) {
			List<Material> mat=new List<Material>();
			foreach(string f in System.IO.Directory.GetFiles (Application.dataPath+"/Materials/VT","*.mat")) {
				mat.Add (AssetDatabase.LoadAssetAtPath (f.Substring (f.IndexOf ("Assets")),typeof(Material)) as Material);
			}
			v.mat=mat.ToArray ();
			v.Init ();
		}
		if(GUILayout.Button ("Make Part")) {
			Undo.RegisterSceneUndo ("Make VT");
			long t0=System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond;
			VTPart p=v.part(v.edx,v.edy);
			for(int i=0;i<v.lod.Length;i++) {
				p.dn=v.lod[i].dn;
				p.Make ();
				string f0="/Resources/VT/LOD"+i,f1="/Part("+v.edx+","+v.edy+")";
				if(!System.IO.Directory.Exists(Application.dataPath+f0+f1))
					System.IO.Directory.CreateDirectory(Application.dataPath+f0+f1);
				SavePart (v.edx,v.edy,f0+f1);
			}
			last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0)/1000;
		}
		EditorGUILayout.LabelField ("Last Make Time:"+last);
		EditorGUILayout.LabelField ("Trans:"+v.part (v.edx,v.edy).transitionMesh.vertexCount);
	}
	
	void OnSceneGUI() {
		v=target as VoxelTerrain;
		Event e=Event.current;
		if(e.type==EventType.keyDown) {
			if(e.control) {
				v.part(v.edx,v.edy).gameObject.SetActive (false);
				if(e.keyCode==KeyCode.RightArrow) v.edx++;
				if(e.keyCode==KeyCode.LeftArrow) v.edx--;
				if(e.keyCode==KeyCode.UpArrow) v.edy++;
				if(e.keyCode==KeyCode.DownArrow) v.edy--;
				v.edx=Mathf.Clamp (v.edx,0,v.nx-1);
				v.edy=Mathf.Clamp (v.edy,0,v.ny-1);
				v.part(v.edx,v.edy).gameObject.SetActive (true);
			}
		}
		Vector3 p=v.partSize;p.Scale (new Vector3(v.edx+0.5f,0.5f,v.edy+0.5f));
		partCube.transform.position=p;
		partCube.transform.localScale=v.partSize;
		v.part (v.edx,v.edy).ManageLOD (v,v.edx,v.edy,Camera.current.transform.position);
		Vector3 cen=v.partSize;cen.Scale (new Vector3(v.edx+0.5f,0.5f,v.edy+0.5f));
		Bounds b0=new Bounds(cen,v.partSize);
		int l=v.part (v.edx,v.edy).getLOD (v,Camera.current.transform.position,b0);
		Handles.BeginGUI ();
		GUI.Label(new Rect(10,10,100,100),"LOD"+l);
		Handles.EndGUI ();
		//if(v.showTrees&&v.tree!=null) {
			//v.DrawTrees ();
		//}
		//if(v.veg!=null) v.veg.SetActive (v.showVeg);
	}
	
	public void SavePart(int x,int y,string path) {
		VTPart vp=v.part(x,y);
		foreach(VTChunk c in vp.chunks) {
			string p=path+"/"+c.gameObject.name+".asset";
			if(System.IO.File.Exists(Application.dataPath+p)) System.IO.File.Delete (Application.dataPath+p);
			AssetDatabase.CreateAsset (c.GetComponent<MeshFilter>().sharedMesh,"Assets"+p);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
			string pp=p.Substring (11);
			pp=pp.Substring (0,pp.IndexOf ("."));
			Object[] o=Resources.LoadAll(pp);
			c.GetComponent<MeshFilter>().sharedMesh=o[0] as Mesh;
		}
	}
}

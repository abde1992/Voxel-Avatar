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
			v.Init ();
		}
		if(GUILayout.Button ("Init Vals")) {
			List<Material> mat=new List<Material>();
			foreach(string f in System.IO.Directory.GetFiles (Application.dataPath+"/Materials/VT","*.mat")) {
				mat.Add (AssetDatabase.LoadAssetAtPath (f.Substring (f.IndexOf ("Assets")),typeof(Material)) as Material);
			}
			List<GameObject> tree=new List<GameObject>();
			foreach(string f in System.IO.Directory.GetFiles (Application.dataPath+"/Models/Trees")) {
				tree.Add (AssetDatabase.LoadAssetAtPath (f.Substring (f.IndexOf ("Assets")),typeof(GameObject)) as GameObject);
			}
			for(int i=0;i<v.nx;i++) {
				for(int j=0;j<v.ny;j++) {
					VTPart p=v.part(i,j);
					p.mat=mat[Random.Range (0,mat.Count)];
					int nt=Random.Range (1,tree.Count);
					p.treePrefabs=new GameObject[nt];
					List<GameObject> tempTree=new List<GameObject>(tree);
					for(int k=0;k<nt;k++) {
						GameObject t=tempTree[Random.Range (0,tempTree.Count)];
						p.treePrefabs[k]=t;tempTree.Remove (t);
					}
				}
			}
		}
		if(GUILayout.Button ("Make Part")) {
			Undo.RegisterSceneUndo ("Make VT");
			long t0=System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond;
			VTPart p=v.part(v.edx,v.edy);
			string f1="/Part("+v.edx+","+v.edy+")";
			EditorUtility.DisplayProgressBar("Making...","",0);
			for(int i=0;i<v.lod.Length;i++) {
				p.Make (v.lod[i].dn);
				string f0="/Resources/VT/LOD"+i;
				EditorUtility.DisplayProgressBar("Making...","",((float)i+0.5f)/(float)v.lod.Length);
				if(!System.IO.Directory.Exists(Application.dataPath+f0+f1))
					System.IO.Directory.CreateDirectory(Application.dataPath+f0+f1);
				SavePart (v.edx,v.edy,f0+f1);
				EditorUtility.DisplayProgressBar("Making...","",(float)(i+1)/(float)v.lod.Length);
			}
			v.part (v.edx,v.edy).MakeTrees ();
			string f2="/Resources/VT/Trees";
			if(!System.IO.Directory.Exists(Application.dataPath+f2+f1))
				System.IO.Directory.CreateDirectory(Application.dataPath+f2+f1);
			//SaveTrees (v.edx,v.edy,f2+f1);
			EditorUtility.ClearProgressBar ();
			last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0)/1000;
		}
		EditorGUILayout.LabelField ("Last Make Time:"+last);
	}
	
	void OnSceneGUI() {
		v=target as VoxelTerrain;
		VTPart part=v.part(v.edx,v.edy);
		Event e=Event.current;
		if(e.type==EventType.keyDown) {
			if(e.control) {
				if(e.keyCode==KeyCode.RightArrow) v.edx++;
				if(e.keyCode==KeyCode.LeftArrow) v.edx--;
				if(e.keyCode==KeyCode.UpArrow) v.edy++;
				if(e.keyCode==KeyCode.DownArrow) v.edy--;
				v.edx=Mathf.Clamp (v.edx,0,v.nx-1);
				v.edy=Mathf.Clamp (v.edy,0,v.ny-1);
				
				Vector3 cp=Camera.current.transform.position;
				v.ManageLOD(cp,v.edx,v.edy);
			}
		}
		if(e.type==EventType.mouseDown) {
			if(e.button==1) {
				v.part(v.edx,v.edy).MakeTransitions ();
			}
		}
		Vector3 p=v.partSize;p.Scale (new Vector3(v.edx+0.5f,0.5f,v.edy+0.5f));
		partCube.transform.position=p;
		partCube.transform.localScale=v.partSize;
		Vector3 cen=v.partSize;cen.Scale (new Vector3(v.edx+0.5f,0.5f,v.edy+0.5f));
		Bounds b0=new Bounds(cen,v.partSize);
		int l=v.part (v.edx,v.edy).getLOD (v,Camera.current.transform.position,b0);
		Handles.BeginGUI ();
		GUI.Label(new Rect(10,10,100,100),"LOD"+l);
		Handles.EndGUI ();
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
	/*
	public void SaveTrees(int x,int y,string path) {
		VTPart vp=v.part(x,y);
		foreach(Transform c in vp.mesh.transform.FindChild ("Trees")) {
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
	}*/
}

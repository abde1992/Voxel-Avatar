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
		v.blockSize=Mathf.NextPowerOfTwo ((int)v.blockSize);
		v.size=Mathf.RoundToInt(v.size/v.npart/v.blockSize)*v.blockSize*v.npart;
		EditorGUILayout.BeginHorizontal ();
		if(GUILayout.Button ("Init")) {
			v.Init ();
			List<Material> mat=new List<Material>();
			foreach(string f in System.IO.Directory.GetFiles (Application.dataPath+"/Materials/VT","*.mat")) {
				mat.Add (AssetDatabase.LoadAssetAtPath (f.Substring (f.IndexOf ("Assets")),typeof(Material)) as Material);
			}
			v.mat=mat[Random.Range (0,mat.Count)];
		}
		EditorGUILayout.EndHorizontal ();
		if(GUILayout.Button ("Make Part")) {
			Undo.RegisterSceneUndo ("Make VT");
			long t0=System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond;
			MakePart (v.edx,v.edy,v.edz,"Making "+v.partName(v.edx,v.edy,v.edz),0,1);
			EditorUtility.ClearProgressBar ();
			last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0)/1000;
		}
		EditorGUILayout.LabelField ("Last Make Time:"+last);
	}
	
	void OnSceneGUI() {
		v=target as VoxelTerrain;
		VTPart part=v.part(v.edx,v.edy,v.edz);
		Event e=Event.current;
		if(e.type==EventType.keyDown) {
			if(e.control) {
				if(e.keyCode==KeyCode.RightArrow) v.edx++;
				if(e.keyCode==KeyCode.LeftArrow) v.edx--;
				if(e.keyCode==KeyCode.UpArrow) v.edz++;
				if(e.keyCode==KeyCode.DownArrow) v.edz--;
				if(e.keyCode==KeyCode.Keypad8) v.edy++;
				if(e.keyCode==KeyCode.Keypad2) v.edy--;
				v.edx=Mathf.Clamp (v.edx,0,v.npart-1);
				v.edy=Mathf.Clamp (v.edy,0,v.npart-1);
				v.edz=Mathf.Clamp (v.edz,0,v.npart-1);
				
				Vector3 cp=Camera.current.transform.position;
				v.ManageLOD(cp);
			}
		}
		Vector3 p=new Vector3(v.edx+0.5f,v.edy+0.5f,v.edz+0.5f)*v.partSize;
		partCube.transform.position=p;
		partCube.transform.localScale=v.partSize*Vector3.one;
		Vector3 cen=new Vector3(v.edx+0.5f,v.edy+0.5f,v.edz+0.5f)*v.partSize;
		Bounds b0=new Bounds(cen,v.partSize*Vector3.one);
		int l=v.part (v.edx,v.edy,v.edz).getLOD (Camera.current.transform.position,b0);
		Handles.BeginGUI ();
		GUI.Label(new Rect(10,10,100,100),"LOD"+l);
		Handles.EndGUI ();
	}
	
	void MakePart(int x,int y,int z,string s,float p0,float p1) {
		VTPart p=v.part(x,y,z);
		string f1="/"+v.partName (x,y,z);
		EditorUtility.DisplayProgressBar(s,"",p0);
		for(int i=0;i<v.lod.Length;i++) {
			p.Make (v.lod[i].dn);
			string f0="/Resources/VT/LOD"+i;
			EditorUtility.DisplayProgressBar(s,"",((float)i+0.5f)/(float)v.lod.Length*(p1-p0)+p0);
			if(!System.IO.Directory.Exists(Application.dataPath+f0+f1))
				System.IO.Directory.CreateDirectory(Application.dataPath+f0+f1);
			SavePart (x,y,z,f0+f1);
			EditorUtility.DisplayProgressBar("Making...","",(float)(i+1)/(float)v.lod.Length*(p1-p0)+p0);
		}
	}
	
	public void SavePart(int x,int y,int z,string path) {
		VTPart vp=v.part(x,y,z);
		foreach(VTChunk c in vp._chunks) {
			if(c==null) continue;
			string p=path+"/"+c.gameObject.name+".asset";
			if(System.IO.File.Exists(Application.dataPath+p)) System.IO.File.Delete (Application.dataPath+p);
			if(AssetDatabase.LoadMainAssetAtPath("Assets"+p)!=null)
				AssetDatabase.DeleteAsset ("Assets"+p);
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

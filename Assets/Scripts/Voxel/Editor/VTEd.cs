using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Voxel Terrain Editor script. Contains inspector and screen editing functionality.
/// </summary>
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
		partCube.SetActive(true);
	}
	void OnDisable() {
		partCube=v.transform.FindChild ("partCube").gameObject;
		partCube.SetActive(false);
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
			MakePart (v.edx,v.edy,v.edz);
			last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0)/1000;
		}
		EditorGUILayout.LabelField ("Time taken in last Make operation:"+last);
		EditorStyles.label.wordWrap = true;
		EditorGUILayout.LabelField("Use the arrow keys, keypad 8 and keypad 2 to select a part of the Voxel Terrain. Once selected, it will" +
			"be highlighted. Then click on 'Make Part' to generate terrain in that region.");

	}
	
	void OnSceneGUI() {
		v=target as VoxelTerrain;
		if(!v.initialized) return;
		VTPart part=v.getPart(v.edx,v.edy,v.edz);
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
				v.LoadLODMeshes(cp);
			}
		}
		Vector3 p=new Vector3(v.edx+0.5f,v.edy+0.5f,v.edz+0.5f)*v.partSize;
		partCube.transform.position=p;
		partCube.transform.localScale=v.partSize*Vector3.one;

		/*
		int l=v.getPart (v.edx,v.edy,v.edz).getLOD (Camera.current.transform.position);
		Handles.BeginGUI ();
		GUI.Label(new Rect(10,10,100,100),"LOD: "+l);
		Handles.EndGUI ();
		*/
	}
	
	void MakePart(int x,int y,int z) {
		VTPart p=v.getPart(x,y,z);
		string makeTitle="Making "+v.partName(x,y,z);
		string saveTitle="Saving "+v.partName(x,y,z);
		string f1="/"+v.partName (x,y,z);
		try {
			EditorUtility.DisplayProgressBar(makeTitle,"",0);
			for(int i=0;i<v.lod.Length;i++) {
				p.Make (v.lod[i].gridSize);
				EditorUtility.DisplayProgressBar(makeTitle,"",((float)i+0.5f)/(float)v.lod.Length);
				string f0="/Resources/VT/LOD"+i;
				if(!System.IO.Directory.Exists(Application.dataPath+f0+f1))
					System.IO.Directory.CreateDirectory(Application.dataPath+f0+f1);
				SavePart (x,y,z,"Assets"+f0+f1);
				EditorUtility.DisplayProgressBar(saveTitle,"",(float)(i+1)/(float)v.lod.Length);
			}
		} catch {
			EditorUtility.ClearProgressBar ();
			throw;
		}
		EditorUtility.ClearProgressBar ();
	}

	/// <summary>
	/// Saves the part at index (x,y,z).
	/// This function is placed in editor script because it calls AssetDatabase functions which are in UnityEditor namespace.
	/// </summary>
	/// <param name="x">The x index.</param>
	/// <param name="y">The y index.</param>
	/// <param name="z">The z index.</param>
	/// <param name="path">Path where the mesh files will be saved (starting from Assets/...).</param>
	public void SavePart(int x,int y,int z,string path) {
		VTPart vp=v.getPart(x,y,z);
		foreach(VTChunk c in vp._chunks) {
			if(c==null) continue;
			string p=path+"/"+c.gameObject.name+".asset";
			string sysPath=p.Substring ("Assets".Length);		// Strip "Assets" from the path
			if(System.IO.File.Exists(Application.dataPath+sysPath))
				System.IO.File.Delete (Application.dataPath+sysPath);
			if(AssetDatabase.LoadMainAssetAtPath(p)!=null)
				AssetDatabase.DeleteAsset (p);
			AssetDatabase.CreateAsset (c.GetComponent<MeshFilter>().sharedMesh,p);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
			// Load back saved mesh.
			// Just a precautionary measure to ensure the mesh has been saved properly.
			string pp=p.Substring ("Assets/Resources/".Length);
			pp=pp.Substring (0,pp.IndexOf ("."));
			Object[] o=Resources.LoadAll(pp);
			c.GetComponent<MeshFilter>().sharedMesh=o[0] as Mesh;
		}
	}
}

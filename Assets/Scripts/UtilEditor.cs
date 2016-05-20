using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class UtilEditor {
	
	/*public static void setTerHeight(Bounds b,float wh) {
		Terrain ter=GameObject.FindObjectOfType(typeof(Terrain)) as Terrain;
		TerrainData td=ter.terrainData;
		float[,] th=td.GetHeights (0,0,td.heightmapWidth,td.heightmapHeight);
		IntPoint ip0=UtilMath.getTerPt (b.min,td),ip1=UtilMath.getTerPt (b.max,td);
		for(int k=ip0.x;k<=ip1.x;k++) {
			for(int n=ip0.y;n<=ip1.y;n++) {
				Vector3 p=UtilMath.getWorldPt (new IntPoint(k,n),td).SetY (600)+Vector3.right*td.heightmapScale.x/2+Vector3.forward*td.heightmapScale.z/2;
				RaycastHit hit;
				if(!Physics.Raycast (p,-Vector3.up,out hit,Mathf.Infinity,1<<LayerMask.NameToLayer ("RoadCol"))) {
					float h=wh/td.heightmapScale.y;
					th[n,k]=h;
				}
			}
		}
		td.SetHeights (0,0,th);
	}
	
	public static float getTerAvgHeight(Bounds b) {
		Terrain ter=GameObject.FindObjectOfType(typeof(Terrain)) as Terrain;
		TerrainData td=ter.terrainData;
		float[,] th=td.GetHeights (0,0,td.heightmapWidth,td.heightmapHeight);
		IntPoint ip0=UtilMath.getTerPt (b.min,td),ip1=UtilMath.getTerPt (b.max,td);
		float h=0;int no=0;
		for(int k=Mathf.Max(0,ip0.x);k<=Mathf.Min(td.heightmapWidth-1,ip1.x);k++) {
			for(int n=Mathf.Max(0,ip0.y);n<=Mathf.Min(td.heightmapHeight-1,ip1.y);n++) {
				h+=th[n,k]*td.heightmapScale.y;no++;
			}
		}
		h/=no;
		return h;
	}
	
	public static void ShowIntPoly(List<IntPoint> hp,Color line,Color sp,float h=0) {
		for(int i=0;i<hp.Count;i++) {
			Vector3 p0=UtilMath.polyWorld (hp[i]).SetY (h);
			Vector3 p1=UtilMath.polyWorld (hp[(i+1)%hp.Count]).SetY (h);
			Handles.color=sp;
			Handles.SphereCap (0,p0,Quaternion.identity,HandleUtility.GetHandleSize (p0)/20);
			Handles.SphereCap (0,p1,Quaternion.identity,HandleUtility.GetHandleSize (p1)/20);
			Handles.color=line;
			Handles.DrawLine (p0,p1);
		}
	}*/
	
	public static float time {
		get {
			float s=((float)System.DateTime.Now.Second%50);
			return (float)System.DateTime.Now.Millisecond+s*1000;
		}
	}
	
	public static object[] CastVecListToobject(Vector3[] p) {
		object[] o=new object[p.Length];
		for(int i=0;i<p.Length;i++) o[i]=p[i];
		return o;
	}
	public delegate Vector3 GetPoint(object o);
	public delegate void SetPoint(Vector3 p,int i);
	public static int SceneSelect(object[] l,GetPoint getPoint,SetPoint setPoint,Event e,int psel,GUIStyle butStyle) {
		for(int i=0;i<l.Length;i++) {
			Vector3 p=getPoint(l[i]);
			Vector2 p2=HandleUtility.WorldToGUIPoint (p);
			if(e.type==EventType.mouseDown&&e.button==0) {
				float d=Vector3.Distance (p2,e.mousePosition);
				if(d<15) psel=i;
			}
			if(psel==i) {
				Vector3 p0=Handles.PositionHandle (p,Quaternion.identity);
				if((p0-p).magnitude>1e-3) setPoint(p0,i);
				Handles.BeginGUI ();
				if(GUI.Button (new Rect(p2.x+10,p2.y+10,20,20),"X",butStyle)) psel=-1;
				Handles.EndGUI ();
			}
		}
		return psel;
	}
	
	public static Texture2D getTexture(Color c) {
		Texture2D t=new Texture2D(1,1,TextureFormat.ARGB32,false);
		t.SetPixel (0,0,c);t.Apply ();return t;
	}
	
	public static void DrawRect(Vector3 p,Vector3 ex,Transform t,int ax,Color face,Color outline) {		// ax -- xy,xz,yz
		Vector3 w=(ax<2?ex.x*Vector3.right:ex.y*Vector3.up);
		Vector3 h=(ax<1?ex.y*Vector3.up:ex.z*Vector3.forward);
		w=t.rotation*w;h=t.rotation*h;p=t.rotation*p+t.position;
		Handles.DrawSolidRectangleWithOutline(new Vector3[] {p-w-h,p+w-h,p+w+h,p-w+h},face,outline);
	}
	
	public static Bounds getLocalBounds(GameObject g) {
		Bounds box=new Bounds(Vector3.zero,Vector3.zero);
		foreach(MeshFilter mf in g.GetComponentsInChildren<MeshFilter>()) {
			Bounds b1=mf.sharedMesh.bounds;
			if(box.size.magnitude<1e-4f) box=b1;
			else box.Encapsulate (b1);
		}return box;
	}
	public static Bounds getWorldBounds(GameObject g) {
		Bounds box=g.renderer!=null?g.renderer.bounds:new Bounds(Vector3.zero,Vector3.zero);
		foreach(MeshRenderer mr in g.GetComponentsInChildren<MeshRenderer>()) {
			Bounds b1=mr.bounds;
			if(box.size.magnitude<1e-4f) box=b1;
			else box.Encapsulate (b1);
		}return box;
	}
	
	public static Mesh CombineMesh(GameObject go,string name,Transform par,List<Material> mat) {
		return CombineMesh (go,name,par,mat,new string[] {},false);
	}
	public static Mesh CombineMesh(GameObject go,string name,Transform par,List<Material> mat,string[] ignoreNames,bool merge) {
		MeshFilter[] mf=go.GetComponentsInChildren<MeshFilter>(true);
		List<CombineInstance> combine = new List<CombineInstance>();
		for (int i=0;i<mf.Length;i++){
			if(name.Length>0&&!mf[i].name.Equals(name)) continue;
			bool cont=false;
			foreach(string s in ignoreNames) if(mf[i].gameObject.name.Contains (s)) {cont=true;break;}
			if(cont) continue;
			CombineInstance ci=new CombineInstance();
			ci.mesh=mf[i].sharedMesh;
			ci.transform = Util.RelativeMatrix (mf[i].transform,par);
			combine.Add(ci);
			if(mat!=null) mat.Add (mf[i].GetComponent<MeshRenderer>().sharedMaterial);
		}
		Mesh m = new Mesh();
		m.CombineMeshes(combine.ToArray (),merge);
		return m;
	}
	public static Mesh CombineMesh(GameObject go,string name,List<Material> mat) {
		MeshFilter[] mf=go.GetComponentsInChildren<MeshFilter>(true);
		List<CombineInstance> combine = new List<CombineInstance>();
		for (int i=0;i<mf.Length;i++){
			if(name.Length>0&&!mf[i].name.Equals(name)) continue;
			CombineInstance ci=new CombineInstance();
			ci.mesh=mf[i].sharedMesh;
			ci.transform = mf[i].transform.localToWorldMatrix;
			combine.Add(ci);
			if(mat!=null) mat.Add (mf[i].GetComponent<MeshRenderer>().sharedMaterial);
		}
		Mesh m = new Mesh();
		m.CombineMeshes(combine.ToArray (),false);
		return m;
	}
	public static void Combine(GameObject go,string name) {
		List<Material> mat=new List<Material>();
		Mesh m=CombineMesh (go,name,mat);
		GameObject g=new GameObject(name+"s");
		//g.transform.position=go.transform.position;g.transform.rotation=go.transform.rotation;
		g.AddComponent<MeshFilter>().sharedMesh=m;
		MeshRenderer mr=g.AddComponent<MeshRenderer>();
		mr.sharedMaterials=mat.ToArray ();
		g.transform.parent=go.transform.parent;
	}
	
	public static Mesh CombineMesh(List<Mesh> mesh) {
		List<CombineInstance> combine = new List<CombineInstance>();
		foreach (Mesh m1 in mesh){
			CombineInstance ci=new CombineInstance();
			ci.mesh=m1;
			combine.Add(ci);
		}
		Mesh m = new Mesh();
		m.CombineMeshes(combine.ToArray (),false);
		return m;
	}
	
	public static GameObject CombineChildren(List<Mesh> mesh,Transform par,string name,Material mat) {
		GameObject g=new GameObject(name);
		g.AddComponent<MeshFilter>();g.AddComponent<MeshRenderer>();
		CombineChildren (g,mesh,par,mat);
		return g;
	}
	public static void CombineChildren(GameObject g,List<Mesh> mesh,Transform par,Material mat) {		// one mesh one material
		List<CombineInstance> combine = new List<CombineInstance>();
		foreach(Mesh mm in mesh) {
			CombineInstance ci=new CombineInstance();
			ci.mesh=mm;
			combine.Add(ci);
		}
		Mesh m = new Mesh();
		m.CombineMeshes(combine.ToArray (),true,false);
		g.transform.parent=par;
		g.transform.localPosition=Vector3.zero;g.transform.localRotation=Quaternion.identity;
		g.transform.localScale=Vector3.one;
		g.GetComponent<MeshFilter>().sharedMesh=m;
		MeshRenderer mr=g.GetComponent<MeshRenderer>();
		mr.sharedMaterial=mat;
	}
	
	public static void setParentTransform(Transform t,Transform par) {
		Vector3 p=t.position;Quaternion q=t.rotation;
		t.parent=par;
		t.position=p;t.rotation=q;
	}
	public static GameObject createOrGetGO(string s,Transform par) {
		if(par==null) {
			GameObject g=GameObject.Find (s);
			if(g==null) g=new GameObject(s);
			return g;
		}
		Transform t=par.FindChild (s);
		if(t==null) t=new GameObject(s).transform;
		t.parent=par;
		return t.gameObject;
	}
	public static GameObject replaceGO(string s,Transform par) {
		Transform t=par.FindChild(s);
		if(t!=null) GameObject.DestroyImmediate (t.gameObject);
		GameObject g=new GameObject(s);
		g.transform.parent=par;
		return g;
	}
	public static GameObject createLocalZeroGO(string s,Transform par) {
		GameObject g=new GameObject(s);
		g.transform.parent=par;
		g.transform.localPosition=Vector3.zero;g.transform.localRotation=Quaternion.identity;
		g.transform.localScale=Vector3.one;
		return g;
	}
	
		
}
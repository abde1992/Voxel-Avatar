using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelTerrain : MonoBehaviour {
	
	public VTPart[] _part;
	public VTPart part(int i,int j) {
		return _part[i*ny+j];
	}
	public Vector3 size=new Vector3(10000,1000,10000);
	public float blockSize=200;
	public Vector3 partSize {
		get {return new Vector3(size.x/(float)nx,size.y,size.z/(float)ny);}
	}
	public int nx=10,ny=10;
	[System.Serializable]
	public class LOD {
		public float d,dn;
	}
	public LOD[] lod;
	[Range(100,1000)]
	public float partCullDis=500,treeCullDis=100;
	
	[HideInInspector] public int edx,edy;
	
	public void Init () {
		_part=new VTPart[nx*ny];
		for(int i=0;i<transform.childCount;) {
			if(transform.GetChild (i).name.StartsWith ("Part")) DestroyImmediate (transform.GetChild (i).gameObject);
			else i++;
		}
		for(int i=0;i<nx;i++) for(int j=0;j<ny;j++) {
			GameObject g=new GameObject("Part("+i+","+j+")");
			VTPart p=_part[i*ny+j]=g.AddComponent<VTPart>();
			p.px=i;p.py=j;
			g.transform.parent=transform;
			g.SetActive (false);
		}
	}
	
	public void ManageLOD(Vector3 cp,int sx,int sy) {
		List<VTPart> lp=new List<VTPart>();
		for(int i=Mathf.Max (0,sx-5);i<Mathf.Min (nx-1,sx+5);i++) {
			for(int j=Mathf.Max (0,sy-5);j<Mathf.Min (ny-1,sy+5);j++) {
				VTPart p=part(i,j);
				p.v=this;
				if(Vector3.Distance (p.cen,cp.SetY(0))>partCullDis) {
					p.OnDisable ();
					p.gameObject.SetActive (false);
					continue;
				}
				p.OnEnable ();
				p.gameObject.SetActive (true);
				p.ManageLOD (cp);
				lp.Add (p);
			}
		}
		foreach(VTPart p in lp) {
			p.MakeTransitions ();
		}
		MakeTrees (cp,lp);
	}
	
	Dictionary<GameObject,List<GameObject>> treePool=new Dictionary<GameObject,List<GameObject>>();
	public void MakeTrees(Vector3 cp,List<VTPart> lp) {
		GameObject mesh=GameObject.Find ("VTMesh");
		Transform par=mesh.transform.FindChild ("Trees");
		if(par==null) {
			par=new GameObject("Trees").transform;
			par.parent=mesh.transform;
		}
		Dictionary<GameObject,List<GameObject>> red=new Dictionary<GameObject, List<GameObject>>(treePool);
		foreach(KeyValuePair<GameObject,List<GameObject>> kv in treePool) red[kv.Key]=new List<GameObject>(kv.Value);
		foreach(VTPart p in lp) {
			foreach(VTPart.TreeGrp t in p.treeGrp) {
				if(Vector3.Distance (cp,t.cen)<treeCullDis) {
					if(!red.ContainsKey (t.pref)) {
						red.Add (t.pref,new List<GameObject>());
						treePool.Add (t.pref,new List<GameObject>());
					}
					foreach(Vector3 pp in t.p) {
						if(red[t.pref].Count>0) {
							GameObject g=red[t.pref][0];
							red[t.pref].RemoveAt(0);
							g.SetActive (true);
							g.transform.position=pp;
						} else {
							GameObject g=GameObject.Instantiate (t.pref) as GameObject;
							treePool[t.pref].Add (g);
							g.transform.parent=par;
							g.transform.position=pp;
						}
					}
				}
			}
		}
		foreach(KeyValuePair<GameObject,List<GameObject>> kv in red)
			foreach(GameObject g in kv.Value) g.SetActive (false);
		
	}
}

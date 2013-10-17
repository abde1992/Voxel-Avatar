using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart {
	
	[System.Serializable]
	public class TreeGrp {
		public List<Vector3> p;
		public Vector3 cen;
		public GameObject pref;
	}
	[HideInInspector] public List<TreeGrp> treeGrp=new List<TreeGrp>();
	public void MakeTrees () {
		Vector3 ex=UtilEditor.getWorldBounds (treePrefabs[0]).extents;
		float exs=2*Mathf.Max (ex.x,ex.z);
		int nx=Mathf.RoundToInt (v.partSize.x/exs);
		int nz=Mathf.RoundToInt (v.partSize.z/exs);
		List<Vector3> treePos=new List<Vector3>();
		for(int i=0;i<nx;i++) {
			for(int j=0;j<nz;j++) {
				if(Random.Range (0f,1f)>0.5f) continue;
				RaycastHit[] hit;
				hit=Physics.RaycastAll (new Vector3((float)i/(float)nx*v.partSize.x,v.partSize.y,(float)j/(float)nz*v.partSize.z),-Vector3.up,v.partSize.y,1<<LayerMask.NameToLayer ("VoxelTerrain"));
				foreach(RaycastHit h in hit) {
					if(Vector3.Dot(h.normal,Vector3.up)>Mathf.Cos(45)) {
						treePos.Add (h.point);
					}
				}
			}
		}
		
		//if(mesh.transform.FindChild ("Trees")!=null) DestroyImmediate (mesh.transform.FindChild ("Trees").gameObject);
		//GameObject par=new GameObject("Trees");
		//par.transform.parent=mesh.transform;
		Octree o=new Octree(v.partSize.x,0,v.partSize.y,0,v.partSize.z,0,100);
		foreach(Vector3 p in treePos) o.AddNode (p,p);
		treeGrp=new List<TreeGrp>();
		List<List<Vector3>> tp=new List<List<Vector3>>();
		o.GetLeavePos (tp);
		foreach(List<Vector3> lp in tp) {
			TreeGrp t=new TreeGrp();
			t.p=lp;
			t.cen=Vector3.zero;
			t.pref=treePrefabs[Random.Range(0,treePrefabs.Length)];
			foreach(Vector3 p in lp) t.cen+=p/lp.Count;
			treeGrp.Add (t);
		}
		/*int it=0;
		foreach(List<Vector3> lp in tp) {
			if(lp.Count==0) continue;
			GameObject tree=treePrefabs[Random.Range (0,treePrefabs.Length)];
			Mesh m=new Mesh();
			Mesh mt=tree.GetComponent<MeshFilter>().sharedMesh;
			List<CombineInstance> ci=new List<CombineInstance>();
			for(int i=0;i<lp.Count;i++) {
				CombineInstance c=new CombineInstance();
				Vector3 sc=new Vector3(Random.Range (0.5f,1.5f),Random.Range (0.5f,1.5f),Random.Range (0.5f,1.5f));
				c.mesh=mt;c.transform=Matrix4x4.TRS (lp[i],Quaternion.Euler (0,Random.Range (0,90),0),sc);
				ci.Add (c);
			}
			m.subMeshCount=mt.subMeshCount;
			for(int i=0;i<mt.subMeshCount;i++) {
				for(int j=0;j<ci.Count;j++) {
					CombineInstance c=ci[j];
					c.subMeshIndex=i;ci[j]=c;
				}
				Mesh m1=new Mesh();
				m1.CombineMeshes (ci.ToArray (),true,true);
				if(i==0) {
					m.vertices=m1.vertices;
					m.uv=m1.uv;m.colors=m1.colors;
					m.normals=m1.normals;m.tangents=m1.tangents;
				}
				m.SetTriangles (m1.triangles,i);
			}
			m.RecalculateBounds ();m.RecalculateNormals ();
			GameObject g=new GameObject();g.name="TreeGrp"+it++;
			g.AddComponent<MeshFilter>().sharedMesh=m;
			g.AddComponent<MeshRenderer>().sharedMaterials=tree.renderer.sharedMaterials;
			g.AddComponent<MeshCollider>().sharedMesh=m;
			g.transform.parent=par.transform;
		}*/
	}
	
	public void MakeVeg () {
		float exs=2;
		int nx=Mathf.RoundToInt (v.partSize.x/exs);
		int nz=Mathf.RoundToInt (v.partSize.z/exs);
		List<Vector3> pos=new List<Vector3>();
		for(int i=0;i<nx;i++) {
			for(int j=0;j<nz;j++) {
				RaycastHit[] hit;
				hit=Physics.RaycastAll (new Vector3((float)i/(float)nx*v.partSize.x,v.partSize.y,(float)j/(float)nz*v.partSize.z),-Vector3.up,v.partSize.y,1<<LayerMask.NameToLayer ("VoxelTerrain"));
				foreach(RaycastHit h in hit) {
					if(Vector3.Dot(h.normal.normalized,Vector3.up)>Mathf.Cos(1)) {
						pos.Add (h.point);
					}
				}
			}
		}
		
		if(mesh.transform.FindChild ("Veg")!=null) DestroyImmediate (mesh.transform.FindChild ("Veg").gameObject);
		GameObject par=new GameObject("Veg");
		par.transform.parent=mesh.transform;
		
		Octree o=new Octree(v.partSize.x,0,v.partSize.y,0,v.partSize.z,0,100);
		foreach(Vector3 p in pos) o.AddNode (p,p);
		List<List<Vector3>> tp=new List<List<Vector3>>();
		o.GetLeavePos (tp);
		foreach(List<Vector3> lp in tp) {
			if(lp.Count==0) continue;
			GameObject g=new GameObject("Grass");
			List<Vector3> vert=new List<Vector3>();
			List<Vector4> tangents=new List<Vector4>();
			List<Vector2> uv=new List<Vector2>();
			List<int> tri=new List<int>();
			System.Func<Vector3,float,float,Vector3,int> addPlane=(p,h,w,nor)=> {
				Vector3 dir=Quaternion.AngleAxis (Random.Range (0f,90f),Vector3.up)*Vector3.forward;
				Vector3 up=-Vector3.Cross (dir,nor);
				vert.Add (p-w*dir);vert.Add (p+w*dir);
				vert.Add (p+up*h-w*dir);vert.Add (p+up*h+w*dir);
				uv.Add (new Vector2(0,0));uv.Add (new Vector2(1,0));
				uv.Add (new Vector2(0,1));uv.Add (new Vector2(1,1));
				Vector3 cen=p+up*h/2;
				Vector4 t=new Vector4(cen.x,cen.y,cen.z,1);
				tangents.Add (t);tangents.Add (t);
				tangents.Add (t);tangents.Add (t);
				tri.Add (vert.Count-4);tri.Add (vert.Count-2);tri.Add (vert.Count-3);
				tri.Add (vert.Count-2);tri.Add (vert.Count-1);tri.Add (vert.Count-3);
				return 0;
			};
			foreach(Vector3 p in lp) {
				for(int i=0;i<5;i++) {
					float h=Random.Range(1f,2f);
					float w=Random.Range (exs/2,exs*2);
					addPlane(p,h,w,Vector3.forward);
				}
				for(int i=0;i<2;i++) {
					float h=Random.Range(1f,2f);
					float w=Random.Range (exs/2,exs*2);
					addPlane(p+Vector3.up*h/2,h,-w,Vector3.up);
				}
			}
			Mesh m=new Mesh();
			m.vertices=vert.ToArray ();m.uv=uv.ToArray();
			m.tangents=tangents.ToArray ();
			m.SetTriangles (tri.ToArray (),0);
			m.RecalculateBounds ();m.RecalculateNormals ();
			g.AddComponent<MeshFilter>().sharedMesh=m;
			g.AddComponent<MeshRenderer>().sharedMaterial=grassMat;
			g.transform.parent=par.transform;
		}
	}
	
}

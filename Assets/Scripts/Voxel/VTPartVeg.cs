using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart {

	public void MakeTrees () {
		Vector3 ex=UtilEditor.getWorldBounds (tree).extents;
		float exs=2*Mathf.Max (ex.x,ex.z);
		int nx=Mathf.RoundToInt (size.x/exs);
		int nz=Mathf.RoundToInt (size.z/exs);
		List<Vector3> treePos=new List<Vector3>();
		for(int i=0;i<nx;i++) {
			for(int j=0;j<nz;j++) {
				RaycastHit[] hit;
				hit=Physics.RaycastAll (new Vector3((float)i/(float)nx*size.x,size.y,(float)j/(float)nz*size.z),-Vector3.up,size.y,1<<LayerMask.NameToLayer ("VoxelTerrain"));
				foreach(RaycastHit h in hit) {
					if(Vector3.Dot(h.normal,Vector3.up)>Mathf.Cos(45)) {
						treePos.Add (h.point);
					}
				}
			}
		}
		
		treeMesh.Clear ();
		Octree o=new Octree(size.x,0,size.y,0,size.z,0,100);
		foreach(Vector3 p in treePos) o.AddNode (p,p);
		List<List<Vector3>> tp=new List<List<Vector3>>();
		o.GetLeavePos (tp);
		foreach(List<Vector3> lp in tp) {
			if(lp.Count==0) continue;
			Mesh m=new Mesh();
			Mesh mt=tree.GetComponent<MeshFilter>().sharedMesh;
			List<CombineInstance> ci=new List<CombineInstance>();
			for(int i=0;i<lp.Count;i++) {
				CombineInstance c=new CombineInstance();
				c.mesh=mt;c.transform=Matrix4x4.TRS (lp[i],Quaternion.identity,Vector3.one);
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
			treeMesh.Add (m);
		}
	}
	
	public void MakeVeg () {
		float exs=2;
		int nx=Mathf.RoundToInt (size.x/exs);
		int nz=Mathf.RoundToInt (size.z/exs);
		List<Vector3> pos=new List<Vector3>();
		for(int i=0;i<nx;i++) {
			for(int j=0;j<nz;j++) {
				RaycastHit[] hit;
				hit=Physics.RaycastAll (new Vector3((float)i/(float)nx*size.x,size.y,(float)j/(float)nz*size.z),-Vector3.up,size.y,1<<LayerMask.NameToLayer ("VoxelTerrain"));
				foreach(RaycastHit h in hit) {
					if(Vector3.Dot(h.normal.normalized,Vector3.up)>Mathf.Cos(1)) {
						pos.Add (h.point);
					}
				}
			}
		}
		
		Octree o=new Octree(size.x,0,size.y,0,size.z,0,100);
		foreach(Vector3 p in pos) o.AddNode (p,p);
		List<List<Vector3>> tp=new List<List<Vector3>>();
		o.GetLeavePos (tp);
		GameObject mesh=GameObject.Find (gameObject.name+" mesh");
		if(veg!=null) DestroyImmediate (veg);
		Transform par=(veg=new GameObject("Veg")).transform;
		par.parent=mesh.transform;
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
			g.transform.parent=par;
		}
	}
	
	
	public void DrawTrees() {
		foreach(Mesh m in treeMesh) {
			Material[] mat=tree.renderer.sharedMaterials;
			for(int i=0;i<m.subMeshCount;i++) {
				Graphics.DrawMesh (m,Vector3.zero,Quaternion.identity,mat[i],LayerMask.NameToLayer ("Default"),Camera.current,i);
			}
		}
	}
	
	public GameObject veg;
}

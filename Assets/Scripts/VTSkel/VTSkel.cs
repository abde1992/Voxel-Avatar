using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VTSkel : MonoBehaviour {
	
	public Vector3 size=new Vector3(200,200,200);
	public float dn=40;
	public void Make () {
		int nx=Mathf.RoundToInt (size.x/dn);
		int ny=Mathf.RoundToInt (size.y/dn);
		int nz=Mathf.RoundToInt (size.z/dn);
		List<Vector3> vert=new List<Vector3>();
		List<int> ind=new List<int>();
		for(int i=0,l=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {
					vert.Add (new Vector3(((float)i+0.5f)/(float)nx*size.x,((float)j+0.5f)/(float)ny*size.y,((float)k+0.5f)/(float)nz*size.z));
					System.Func<int,int,int,int> getInd=(a,b,c)=>a*ny*nz+b*nz+c;
					/*if(i<nx-1&&j<ny-1&&k<nz-1) {
						ind.Add (getInd(i,j,k));ind.Add (getInd(i+1,j,k));ind.Add (getInd(i,j+1,k));
						ind.Add (getInd(i,j+1,k));ind.Add (getInd(i+1,j,k));ind.Add (getInd(i+1,j+1,k));
						ind.Add (getInd(i,j,k));ind.Add (getInd(i,j,k+1));ind.Add (getInd(i,j+1,k));
						ind.Add (getInd(i,j+1,k));ind.Add (getInd(i,j,k+1));ind.Add (getInd(i,j+1,k+1));
						ind.Add (getInd(i,j,k));ind.Add (getInd(i+1,j,k));ind.Add (getInd(i,j,k+1));
						ind.Add (getInd(i,j,k+1));ind.Add (getInd(i+1,j,k));ind.Add (getInd(i+1,j,k+1));
					}*/
					ind.Add (l++);
				}
			}
		}
		Mesh m=new Mesh();
		m.vertices=vert.ToArray ();
		m.SetIndices (ind.ToArray (),MeshTopology.Points,0);
		m.RecalculateBounds ();m.RecalculateNormals ();
		GetComponent<MeshFilter>().sharedMesh=m;
	}
}

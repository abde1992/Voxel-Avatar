using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VTSkelCPU : MonoBehaviour {
	
	public Vector3 size=new Vector3(200,1000,200);
	public float blockSize=40;
	public VoxelTerrain.LOD[] lod;
	public float dny=100;
	public Material mat;
	public Transform target;
	
	float Density(Vector3 p) {
		/*Vector3 v=p-size/2;v.y=0;
		float h=100+peakHt-v.magnitude/size.magnitude*2*peakHt;
		float d=(h-p.y)*(p.y-100);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.x)-size.x*0.4f,0f,Mathf.Infinity);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.z)-size.z*0.4f,0f,Mathf.Infinity);*/
		float d=size.y/2-p.y;
		float f=0.00632f/7,a=size.y/4;
		System.Func<float,float,float> NLQs=(mf,ma)=>a*ma*(SimplexNoise.Noise.Generate (p*f*mf));
		System.Func<float,float,Vector3,float> NLQs1=(mf,ma,p1)=>a*ma*(SimplexNoise.Noise.Generate (p1*f*mf));
		System.Func<float,float,float> NLQu=(mf,ma)=>a*ma*(SimplexNoise.Noise.Generate (p*f*mf)+1)/2;
		//for(int i=0,m=1;i<9;i++,m*=2) {
			//d+=40/m*SimplexNoise.Noise.Generate (p*0.00632f*m);
		//}
		d+=NLQs(512 * 0.934f, 4.793e-4f)
			 + NLQs(256 * 1.021f,0.000793f)
			 + NLQs(128 * 0.985f, 0.001792f)
             + NLQs(64 * 1.051f, 0.003456f)
             + NLQs(32 * 1.020f, 0.06656f)
             + NLQs(16 * 0.968f, 0.125f)
             + NLQs(8 * 0.994f, 0.25f * 1.0f)
             + NLQs(4*1.045f, 0.5f*0.9f)
             + NLQs(2*0.972f, 0.8f)
             + NLQs1(1f, 1f, p+new Vector3(NLQu(1f,8f),NLQu(1.95f,4f),NLQu(4.1f,2f)));
		
		float hard_floor_y = 500;
		d += Mathf.Clamp01((hard_floor_y - p.y)*3)*40;  
		return d;
	}
	
	public class int3 {
		public int x,y,z;
		public int icase;
		public float[] ed;
		public int3(int tx,int ty,int tz) {x=tx;y=ty;z=tz;}
	}
	[HideInInspector] public List<int3>[,,] filled_cells;
	public void Make() {
		Vector3 p=target.position;
		for(int i=0;i<transform.childCount;) DestroyImmediate (transform.GetChild (0).gameObject);
		int nx=Mathf.RoundToInt (size.x/blockSize);
		int ny=Mathf.RoundToInt (size.y/blockSize);
		int nz=Mathf.RoundToInt (size.z/blockSize);
		long t0=System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond;
		if(filled_cells==null||filled_cells.GetLength (0)!=nx||filled_cells.GetLength (1)!=ny||filled_cells.GetLength (2)!=nz) {
			filled_cells=new List<int3>[nx,ny,nz];
		}
		int nfc=0;
		for(int i=0,gi=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {
					if(filled_cells[i,j,k]!=null) nfc+=filled_cells[i,j,k].Count;
					Vector3 or=blockSize*new Vector3(i,j,k);
					Vector3 cen=or+blockSize*Vector3.one/2;
					float d=lod[lod.Length-1].dn;
					for(int a=0;a<lod.Length;a++) if(lod[a].d<Vector3.Distance (p,cen)) {d=lod[a].dn;break;}
					Mesh m=Make (or,blockSize*Vector3.one,d,ref filled_cells[i,j,k]);
					if(m==null) continue;
					GameObject g=new GameObject("Chunk"+gi++);
					g.transform.parent=transform;
					g.AddComponent<MeshFilter>().sharedMesh=m;
					g.AddComponent<MeshRenderer>().sharedMaterial=mat;
					g.AddComponent<MeshCollider>().sharedMesh=m;
					g.layer=LayerMask.NameToLayer ("VoxelTerrain");
				}
			}
		}
		float last=(System.DateTime.Now.Ticks/System.TimeSpan.TicksPerMillisecond-t0);
		Debug.Log (last+":"+nfc);
	}
	Mesh Make(Vector3 or,Vector3 size,float dn,ref List<int3> fc) {
		if(fc!=null&&fc.Count==0) return null;
		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();
		int nx=Mathf.CeilToInt (size.x/dn);
		int ny=Mathf.CeilToInt (size.y/dny);
		int nz=Mathf.CeilToInt (size.z/dn);
		if(fc==null) {
			fc=new List<int3>();
			System.Func<int,int,int,Vector3> gridPt=(a,b,c)=> {
				return or+new Vector3((float)a/(float)nx*size.x,(float)b/(float)ny*size.y,(float)c/(float)nz*size.z);
			};
			float[,,] density=new float[nx+2,ny+2,nz+2];
			for(int a=0;a<=nx+1;a++) for(int b=0;b<=ny+1;b++) for(int c=0;c<=nz+1;c++) {
				density[a,b,c]=Density (gridPt(a,b,c));
			}
			int[,,,] iv=new int[nx+1,ny+1,nz+1,3];
			for(int a=0;a<nx+1;a++) for(int b=0;b<ny+1;b++) for(int c=0;c<nz+1;c++) {
				System.Func<int,int,int,int> getEdgePt=(i,j,k)=> {
					float d0=density[a,b,c];
					float d1=density[a+i,b+j,c+k];
					if(d0*d1>=0) {
						return -1;
					}
					Vector3 p0=gridPt(a,b,c);
					Vector3 p1=gridPt(a+i,b+j,c+k);
					vert.Add(p0+d0/(d0-d1)*(p1-p0));
					return vert.Count-1;
				};
				iv[a,b,c,0]=getEdgePt(1,0,0);
				iv[a,b,c,1]=getEdgePt(0,1,0);
				iv[a,b,c,2]=getEdgePt(0,0,1);
			}
			for(int i=0;i<nx;i++) {
				for(int j=0;j<ny;j++) {
					for(int k=0;k<nz;k++) {
						int icase=0;
						for(int l=0,t=1;l<8;l++,t*=2) {
							if(density[i+VTPart.edgeVerts[l*3+0],j+VTPart.edgeVerts[l*3+1],k+VTPart.edgeVerts[l*3+2]]>0) icase|=t;
						}
						if(VTPart.numPolys[icase]==0) continue;
						int3 fi=new int3(i,j,k);fi.icase=icase;
						fi.ed=new float[VTPart.numPolys[icase]*3];
						System.Func<int,int,int,int> getEdgeVert=(ed,l,m)=> {
							int i0=i+VTPart.edgeStart[ed*3+0],j0=j+VTPart.edgeStart[ed*3+1],k0=k+VTPart.edgeStart[ed*3+2];
							int i1=VTPart.edgeDir[ed*3+0],j1=VTPart.edgeDir[ed*3+1],k1=VTPart.edgeDir[ed*3+2];
							Vector3 p0=gridPt(i0,j0,k0);
							Vector3 p1=gridPt(i0+i1,j0+j1,k0+k1);
							int iv0=iv[i0,j0,k0,(i1+j1*2+k1*3)-1];
							Vector3 p=vert[iv0];
							fi.ed[l*3+m]=(p-p0).magnitude/(p1-p0).magnitude;
							return iv0;
						};
						for(int l=0;l<VTPart.numPolys[icase];l++) {
							for(int m=0;m<3;m++) {
								int ed=VTPart.polys[icase*15+l*3+m];
								//vert.Add (ptOnEdge(ed));
								float d;
								int iv0=getEdgeVert(ed,l,m);
								tri.Add (iv0);
							}
						}
						fc.Add (fi);
					}
				}
			}
		}
		else {
			System.Func<int,int,int,Vector3> gridPt=(a,b,c)=> {
				return or+new Vector3((float)a/(float)nx*size.x,(float)b/(float)ny*size.y,(float)c/(float)nz*size.z);
			};
			//int[,,,] iv=new int[nx+1,ny+1,nz+1,3];
			//for(int a=0;a<nx+1;a++) for(int b=0;b<ny+1;b++) for(int c=0;c<nz+1;c++) for(int d=0;d<3;d++) iv[a,b,c,d]=-1;
			/*for(int a=0;a<nx+1;a++) for(int b=0;b<ny+1;b++) for(int c=0;c<nz+1;c++) {
				System.Func<int,int,int,int> getEdgePt=(i,j,k)=> {
					if(a+i>nx||b+j>ny||c+k>nz) return -1;
					float d0=density[a,b,c];
					float d1=density[a+i,b+j,c+k];
					if(d0*d1>=0) {
						return -1;
					}
					Vector3 p0=gridPt(a,b,c);
					Vector3 p1=gridPt(a+i,b+j,c+k);
					vert.Add(p0+d0/(d0-d1)*(p1-p0));
					return vert.Count-1;
				};
				iv[a,b,c,0]=getEdgePt(1,0,0);
				iv[a,b,c,1]=getEdgePt(0,1,0);
				iv[a,b,c,2]=getEdgePt(0,0,1);
			}*/
			foreach(int3 fi in fc) {
				int i=fi.x,j=fi.y,k=fi.z;
				int icase=fi.icase;
				System.Func<int,int,int,int> getEdgeVert=(ed,l,m)=> {
					int i0=(i+VTPart.edgeStart[ed*3+0]),j0=(j+VTPart.edgeStart[ed*3+1]),k0=(k+VTPart.edgeStart[ed*3+2]);
					int i1=VTPart.edgeDir[ed*3+0],j1=VTPart.edgeDir[ed*3+1],k1=VTPart.edgeDir[ed*3+2];
					//int iv0=iv[i0,j0,k0,(i1+j1*2+k1*3)-1];
					//if(iv0!=-1) return iv0;
					Vector3 p0=gridPt(i0,j0,k0);
					Vector3 p1=gridPt(i1+i0,j1+j0,k1+k0);
					vert.Add(p0+(p1-p0)*fi.ed[l*3+m]);
					//iv[i0,j0,k0,(i1+j1*2+k1*3)-1]=vert.Count-1;
					return vert.Count-1;
				};
				for(int l=0;l<VTPart.numPolys[icase];l++) {
					for(int m=0;m<3;m++) {
						int ed=VTPart.polys[icase*15+l*3+m];
						//vert.Add (ptOnEdge(ed));
						int iv0=getEdgeVert(ed,l,m);
						tri.Add (iv0);
					}
				}
			}
		}
		
		if(tri.Count==0) return null;
		
		Mesh mesh=new Mesh();
		mesh.vertices=vert.ToArray ();
		mesh.SetTriangles(tri.ToArray (),0);
		mesh.RecalculateBounds ();mesh.RecalculateNormals ();
		
		return mesh;
	}
	
}

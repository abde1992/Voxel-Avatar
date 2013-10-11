using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart : MonoBehaviour {
	
	[HideInInspector] public Vector3 size=new Vector3(1000,600,1000);
	public float dn=4,blockSize=200;
	public Material mat,grassMat;
	public GameObject mesh,tree;
	public List<VTChunk> chunks=new List<VTChunk>();
	public List<Mesh> treeMesh=new List<Mesh>();
	
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
	
	public void Make() {
		for(int i=0;i<transform.childCount;) DestroyImmediate (transform.GetChild (0).gameObject);
		mesh=GameObject.Find (gameObject.name+" mesh");
		if(mesh!=null) DestroyImmediate (mesh);
		mesh=new GameObject(gameObject.name+" mesh");
		mesh.transform.parent=UtilEditor.createOrGetGO ("VTMesh",null).transform;
		blockSize=Mathf.NextPowerOfTwo ((int)blockSize);
		int nx=Mathf.RoundToInt (size.x/blockSize);
		int ny=Mathf.RoundToInt (size.y/blockSize);
		int nz=Mathf.RoundToInt (size.z/blockSize);
		chunks.Clear ();
		VTChunk[,,] nch=new VTChunk[nx,ny,nz];
		for(int i=0,gi=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {
					Mesh m=Make (blockSize*new Vector3(i,j,k),blockSize*Vector3.one);
					if(m==null) continue;
					GameObject g=new GameObject("Chunk"+gi++);
					g.transform.parent=mesh.transform;
					g.AddComponent<MeshFilter>().sharedMesh=m;
					g.AddComponent<MeshRenderer>().sharedMaterial=mat;
					g.AddComponent<MeshCollider>().sharedMesh=m;
					g.layer=LayerMask.NameToLayer ("VoxelTerrain");
					VTChunk c=g.AddComponent<VTChunk>();
					c.x=i;c.y=j;c.z=k;
					System.Func<int,int,int,int> SetNeighs=(x,y,z)=> {
						VTChunk c1=nch[i+x,j+y,k+z];
						if(c1!=null) {c.setNeigh (x,y,z,c1);c1.setNeigh (-x,-y,-z,c);}
						return 0;
					};
					if(i>0) SetNeighs(-1,0,0);
					if(j>0) SetNeighs(0,-1,0);
					if(k>0) SetNeighs(0,0,-1);
					nch[i,j,k]=c;
					chunks.Add (c);
				}
			}
		}
	}
	Mesh Make(Vector3 or,Vector3 size) {
		int nx=Mathf.RoundToInt (size.x/dn);
		int ny=Mathf.RoundToInt (size.y/dn);
		int nz=Mathf.RoundToInt (size.z/dn);
		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();
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
				Vector3 p0=gridPt(a,b,c);
				Vector3 p1=gridPt(a+i,b+j,c+k);
				float d0=density[a,b,c];
				float d1=density[a+i,b+j,c+k];
				if(d0*d1>=0) {
					return -1;
				}
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
						if(density[i+edgeVerts[l*3+0],j+edgeVerts[l*3+1],k+edgeVerts[l*3+2]]>0) icase|=t;
					}
					System.Func<int,int> getEdgeVert=(ed)=> {
						return iv[(i+edgeStart[ed*3+0]),(j+edgeStart[ed*3+1]),(k+edgeStart[ed*3+2]),
							(edgeDir[ed*3+0]+edgeDir[ed*3+1]*2+edgeDir[ed*3+2]*3)-1];
					};
					for(int l=0;l<numPolys[icase];l++) {
						for(int m=0;m<3;m++) {
							int ed=polys[icase*15+l*3+m];
							//vert.Add (ptOnEdge(ed));
							int iv0=getEdgeVert(ed);
							tri.Add (iv0);
						}
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
	
	public int getLOD(VoxelTerrain v,Vector3 p,Bounds b) {
		float d=Vector3.Distance(p,b.center)-b.extents.Max ();
		for(int i=0;i<v.lod.Length;i++) if(d<v.lod[i].d) return i;
		return v.lod.Length-1;
	}
	public void ManageLOD(VoxelTerrain v,int x,int y,Vector3 p) {
		Vector3 cen=v.partSize;cen.Scale (new Vector3(x+0.5f,0.5f,y+0.5f));
		Bounds b0=new Bounds(cen,v.partSize);
		foreach(VTChunk c in chunks) {
			MeshFilter mf=c.GetComponent<MeshFilter>();
			int l=c.lod=getLOD(v,p,mf.sharedMesh!=null?mf.sharedMesh.bounds:b0);
			string path="VT/LOD"+l+"/Part("+x+","+y+")/"+c.gameObject.name+".asset";
			path=path.Substring (0,path.IndexOf ("."));
			Mesh m=Resources.Load(path,typeof(Mesh)) as Mesh;
			mf.sharedMesh=m as Mesh;
			c.GetComponent<MeshCollider>().sharedMesh=m;
		}
		
		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();
		foreach(VTChunk c in chunks) {
			Vector3 or=blockSize*new Vector3(c.x,c.y,c.z),size=blockSize*Vector3.one;
			int nx=Mathf.RoundToInt (size.x/v.lod[c.lod].dn);
			int ny=Mathf.RoundToInt (size.y/v.lod[c.lod].dn);
			int nz=Mathf.RoundToInt (size.z/v.lod[c.lod].dn);
			if(c.getNeigh (0,1,0)!=null&&c.lod==c.getNeigh (0,1,0).lod+1) {
				for(int i=0;i<nx;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(i+(float)a/2)/(float)nx*size.x,(float)(ny)/(float)ny*size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							if(!invert) {
								tri.Add (vert.Count-2);tri.Add (vert.Count-3);tri.Add (vert.Count-1);
							} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							}
						}
					}
				}
			}
		}
		if(vert.Count==0) return;
		transitionMesh=new Mesh();
		transitionMesh.vertices=vert.ToArray ();
		transitionMesh.SetTriangles (tri.ToArray (),0);
		transitionMesh.RecalculateBounds ();transitionMesh.RecalculateNormals ();
		GameObject tr=UtilEditor.createOrGetGO ("Tr",transform);
		if(tr.GetComponent<MeshFilter>()==null) tr.AddComponent<MeshFilter>();
		if(tr.GetComponent<MeshRenderer>()==null) tr.AddComponent<MeshRenderer>();
		tr.GetComponent<MeshFilter>().sharedMesh=transitionMesh;
		tr.GetComponent<MeshRenderer>().sharedMaterial=mat;
	}
	
	public Mesh transitionMesh;
	void Update() {
		DrawTrees ();
		if(transitionMesh!=null)
			;//Graphics.DrawMesh (transitionMesh,Vector3.zero,Quaternion.identity,mat,LayerMask.NameToLayer ("VoxelTerrain"));
	}
	
}

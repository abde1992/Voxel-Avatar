using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart : MonoBehaviour {
	
	[HideInInspector] public int px,py;
	public Material mat,grassMat;
	public GameObject mesh;
	public GameObject[] treePrefabs;
	public List<VTChunk> chunks=new List<VTChunk>();
	
	float Density(Vector3 p) {
		/*Vector3 v=p-size/2;v.y=0;
		float h=100+peakHt-v.magnitude/size.magnitude*2*peakHt;
		float d=(h-p.y)*(p.y-100);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.x)-size.x*0.4f,0f,Mathf.Infinity);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.z)-size.z*0.4f,0f,Mathf.Infinity);*/
		float d=v.partSize.y/2-p.y;
		float f=0.00632f/7,a=v.partSize.y/4;
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
	
	public void Make(float dn) {
		for(int i=0;i<transform.childCount;) DestroyImmediate (transform.GetChild (0).gameObject);
		mesh=GameObject.Find (gameObject.name+" mesh");
		if(mesh!=null) DestroyImmediate (mesh);
		mesh=new GameObject(gameObject.name+" mesh");
		mesh.transform.parent=UtilEditor.createOrGetGO ("VTMesh",null).transform;
		v.blockSize=Mathf.NextPowerOfTwo ((int)v.blockSize);
		Vector3 size=v.partSize;
		int nx=Mathf.RoundToInt (size.x/v.blockSize);
		int ny=Mathf.RoundToInt (size.y/v.blockSize);
		int nz=Mathf.RoundToInt (size.z/v.blockSize);
		chunks.Clear ();
		VTChunk[,,] nch=new VTChunk[nx+2,ny+2,nz+2];
		for(int i=0;i<2;i++) {
			if(px+i*2-1>=v.nx||px+i*2-1<0) continue;
			for(int j=0;j<ny;j++)
				for(int k=0;k<nz;k++) {
					VTPart p=v.part (px+i*2-1,py);
					foreach(VTChunk c in p.chunks) {
						if(c.x==((i+1)%2)*(nx-1)&&c.y==j&&c.z==k) {nch[i*nx,j+1,k+1]=c;break;}
					}
				}
		}
		for(int i=0;i<nx;i++)
			for(int j=0;j<ny;j++)
				for(int k=0;k<2;k++) {
					if(py+k*2-1>=v.ny||py+k*2-1<0) continue;
					VTPart p=v.part (px,py+k*2-1);
					foreach(VTChunk c in p.chunks) {
						if(c.z==((k+1)%2)*(nz-1)&&c.y==j&&c.x==i) {nch[i+1,j+1,k*nz]=c;break;}
					}
				}
		for(int i=0,gi=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {
					Mesh m=Make (dn,origin+v.blockSize*new Vector3(i,j,k),v.blockSize*Vector3.one);
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
						VTChunk c1=nch[i+1+x,j+1+y,k+1+z];
						if(c1!=null) {c.setNeigh (x,y,z,c1);c1.setNeigh (-x,-y,-z,c);}
						return 0;
					};
					SetNeighs(-1,0,0);SetNeighs(1,0,0);
					SetNeighs(0,-1,0);SetNeighs(0,1,0);
					SetNeighs(0,0,-1);SetNeighs(0,0,1);
					nch[i+1,j+1,k+1]=c;
					chunks.Add (c);
				}
			}
		}
	}
	Mesh Make(float dn,Vector3 or,Vector3 size) {
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
	public void ManageLOD(Vector3 p) {
		Vector3 cen=v.partSize;cen.Scale (new Vector3(px+0.5f,0.5f,py+0.5f));
		Bounds b0=new Bounds(cen,v.partSize);
		foreach(VTChunk c in chunks) {
			MeshFilter mf=c.GetComponent<MeshFilter>();
			int l=c.lod=getLOD(v,p,mf.sharedMesh!=null?mf.sharedMesh.bounds:b0);
			string path="VT/LOD"+l+"/Part("+px+","+py+")/"+c.gameObject.name+".asset";
			path=path.Substring (0,path.IndexOf ("."));
			Mesh m=Resources.Load(path,typeof(Mesh)) as Mesh;
			mf.sharedMesh=m as Mesh;
			c.GetComponent<MeshCollider>().sharedMesh=m;
		}
	}
	
	[HideInInspector] public VoxelTerrain v;
	public void Update() {
	}
	
	public void OnEnable() {
		if(mesh==null) return;
		v=transform.parent.GetComponent<VoxelTerrain>();
		mesh.SetActive (true);
	}
	public void OnDisable() {
		if(mesh==null) return;
		mesh.SetActive (false);
	}
	
	public Vector3 cen {
		get {
			Vector3 size=v.partSize;
			return new Vector3(((float)px+0.5f)*size.x,0,((float)py+0.5f)*size.z);
		}
	}
	public Vector3 origin {
		get {
			return new Vector3(px*v.partSize.x,0,py*v.partSize.z);
		}
	}
	
}

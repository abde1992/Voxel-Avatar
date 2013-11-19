using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart : MonoBehaviour {
	
	[HideInInspector] public VoxelTerrain v;
	[HideInInspector] public int px,py,pz;
	public GameObject mesh;
	int nchunk {get{return Mathf.RoundToInt (v.partSize/v.blockSize);}}
	public VTChunk[] _chunks;
	public VTChunk chunk(int x,int y,int z) {
		if(_chunks==null||_chunks.Length==0) return null;
		return _chunks[x*nchunk*nchunk+y*nchunk+z];
	}
	public void chunk(int x,int y,int z,VTChunk c) {
		_chunks[x*nchunk*nchunk+y*nchunk+z]=c;
	}
	
	public void Make(float dn) {
		mesh=GameObject.Find (gameObject.name+" mesh");
		if(mesh!=null) DestroyImmediate (mesh);
		mesh=new GameObject(gameObject.name+" mesh");
		mesh.transform.parent=UtilEditor.createOrGetGO ("VTMesh",null).transform;
		_chunks=new VTChunk[nchunk*nchunk*nchunk];
		VTChunk[,,] nch=new VTChunk[nchunk,nchunk,nchunk];
		for(int i=0,gi=0;i<nchunk;i++) {
			for(int j=0;j<nchunk;j++) {
				for(int k=0;k<nchunk;k++) {
					Mesh m=Make (dn,origin+v.blockSize*new Vector3(i,j,k),v.blockSize*Vector3.one);
					if(m==null) {chunk (i,j,k,null);continue;}
					GameObject g=new GameObject("Chunk"+gi++);
					g.transform.parent=mesh.transform;
					g.AddComponent<MeshFilter>().sharedMesh=m;
					g.AddComponent<MeshRenderer>().sharedMaterial=v.mat;
					g.AddComponent<MeshCollider>().sharedMesh=m;
					g.layer=LayerMask.NameToLayer ("VoxelTerrain");
					VTChunk c=g.AddComponent<VTChunk>();
					c.x=i;c.y=j;c.z=k;
					System.Func<int,int,int,int> SetNeighs=(x,y,z)=> {
						VTChunk c1=null;
						if(i+x==-1) c1=px<=0?null:v.part (px-1,py,pz).chunk(nchunk-1,j,k);
						else if(i+x==nchunk) c1=px>=v.npart-1?null:v.part (px+1,py,pz).chunk(0,j,k);
						else if(j+y==-1) c1=py<=0?null:v.part (px,py-1,pz).chunk(i,nchunk-1,k);
						else if(j+y==nchunk) c1=py>=v.npart-1?null:v.part (px,py+1,pz).chunk(i,0,k);
						else if(k+z==-1) c1=pz<=0?null:v.part (px,py,pz-1).chunk(i,j,nchunk-1);
						else if(k+z==nchunk) c1=pz>=v.npart-1?null:v.part (px,py,pz+1).chunk(i,j,0);
						else {
							c1=nch[i+x,j+y,k+z];
						}
						if(c1!=null) {c.setNeigh (x,y,z,c1);c1.setNeigh (-x,-y,-z,c);}
						return 0;
					};
					SetNeighs(-1,0,0);SetNeighs(1,0,0);
					SetNeighs(0,-1,0);SetNeighs(0,1,0);
					SetNeighs(0,0,-1);SetNeighs(0,0,1);
					nch[i,j,k]=c;
					chunk(i,j,k,c);
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
			density[a,b,c]=v.Density (gridPt(a,b,c));
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
	
	public int getLOD(Vector3 p,Bounds b) {
		float d=Vector3.Distance(p,b.center)-b.extents.Max ();
		for(int i=0;i<v.lod.Length;i++) if(d<v.lod[i].d) return i;
		return v.lod.Length-1;
	}
	public void ManageLOD(Vector3 p) {
		Vector3 cen=new Vector3(px+0.5f,py+0.5f,pz+0.5f)*v.partSize;
		Bounds b0=new Bounds(cen,v.partSize*Vector3.one);
		foreach(VTChunk c in _chunks) {
			if(c==null) continue;
			MeshFilter mf=c.GetComponent<MeshFilter>();
			int l=c.lod=getLOD(p,mf.sharedMesh!=null?mf.sharedMesh.bounds:b0);
			string path="VT/LOD"+l+"/"+v.partName (px,py,pz)+"/"+c.gameObject.name+".asset";
			path=path.Substring (0,path.IndexOf ("."));
			Mesh m=Resources.Load(path,typeof(Mesh)) as Mesh;
			mf.sharedMesh=m as Mesh;
			c.GetComponent<MeshCollider>().sharedMesh=m;
		}
	}
	
	public void Update() {
	}
	
	public Vector3 cen {
		get {
			float size=v.partSize;
			return new Vector3(((float)px+0.5f)*size,((float)py+0.5f)*size,((float)pz+0.5f)*size);
		}
	}
	public Vector3 origin {
		get {
			return new Vector3(px*v.partSize,py*v.partSize,pz*v.partSize);
		}
	}
	
}

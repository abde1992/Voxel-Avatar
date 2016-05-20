using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart : MonoBehaviour {

	// VoxelTerrain object of which it is a part of
	[SerializeField] private VoxelTerrain v;
	// Indices of this part in the VoxelTerrain
	[HideInInspector] public int px,py,pz;
	public GameObject mesh;
	// No. of chunks this part can have
	private int nchunk {get{return Mathf.RoundToInt (v.partSize/v.blockSize);}}
	public VTChunk[] _chunks;
	public VTChunk chunk(int x,int y,int z) {
		if(_chunks==null||_chunks.Length==0) return null;
		return _chunks[x*nchunk*nchunk+y*nchunk+z];
	}
	public void chunk(int x,int y,int z,VTChunk c) {
		_chunks[x*nchunk*nchunk+y*nchunk+z]=c;
	}

	/// <summary>
	/// Initialize function. Called from VoxelTerrain when a new VTPart gameobject is instantiated
	/// </summary>
	/// <param name="v">VoxelTerrain parent</param>
	/// <param name="i"> Index of this part in x direction </param>
	/// <param name="j"> Index of this part in y direction </param>
	/// <param name="k"> Index of this part in z direction </param>
	public void Init(VoxelTerrain v,int i,int j,int k) {
		this.v=v;
		px=i;py=j;pz=k;
		// Init chunks array
		_chunks=new VTChunk[nchunk*nchunk*nchunk];
	}

	/// <summary>
	/// Make all meshes in the area of this part of the terrain with the specified gridSize
	/// </summary>
	/// <param name="gridSize">Grid size (in meters)</param>
	public void Make(float gridSize) {
		// Get the mesh Gameobject if already present in the hierarchy, or create a new one.
		mesh=GameObject.Find (gameObject.name+" mesh");
		if(mesh!=null) DestroyImmediate (mesh);
		mesh=new GameObject(gameObject.name+" mesh");
		mesh.transform.parent=UtilEditor.createOrGetGO ("VTMesh",null).transform;

		VTChunk[,,] nch=new VTChunk[nchunk,nchunk,nchunk];
		for(int i=0,gi=0;i<nchunk;i++) {
			for(int j=0;j<nchunk;j++) {
				for(int k=0;k<nchunk;k++) {
					Mesh m=Make (gridSize,origin+v.blockSize*new Vector3(i,j,k),v.blockSize*Vector3.one);
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
						if(i+x==-1) c1=px<=0?null:v.getPart (px-1,py,pz).chunk(nchunk-1,j,k);
						else if(i+x==nchunk) c1=px>=v.npart-1?null:v.getPart (px+1,py,pz).chunk(0,j,k);
						else if(j+y==-1) c1=py<=0?null:v.getPart (px,py-1,pz).chunk(i,nchunk-1,k);
						else if(j+y==nchunk) c1=py>=v.npart-1?null:v.getPart (px,py+1,pz).chunk(i,0,k);
						else if(k+z==-1) c1=pz<=0?null:v.getPart (px,py,pz-1).chunk(i,j,nchunk-1);
						else if(k+z==nchunk) c1=pz>=v.npart-1?null:v.getPart (px,py,pz+1).chunk(i,j,0);
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

	/// <summary>
	/// Makes the terrain of a certain volume at the specified gridSize.
	/// Origin and size represent the bounds of this volume
	/// </summary>
	/// <param name="gridSize">Grid size</param>
	/// <param name="volumeOrigin">Origin of the volume</param>
	/// <param name="volumeSize">Size of the volume</param>
	private Mesh Make(float dn,Vector3 volumeOrigin,Vector3 volumeSize) {

		// Lists to store the mesh attributes
		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();

		// Dimensions of the grid
		int nx=Mathf.RoundToInt (volumeSize.x/dn);
		int ny=Mathf.RoundToInt (volumeSize.y/dn);
		int nz=Mathf.RoundToInt (volumeSize.z/dn);

		// Function to calculate position on the grid from index (a,b,c)
		System.Func<int,int,int,Vector3> gridPt=(a,b,c)=> {
			return volumeOrigin+new Vector3((float)a/(float)nx*volumeSize.x,(float)b/(float)ny*volumeSize.y,(float)c/(float)nz*volumeSize.z);
		};

		// Precalculate density function on the grid points. Very expensive.
		float[,,] density=new float[nx+2,ny+2,nz+2];
		for(int a=0;a<=nx+1;a++) for(int b=0;b<=ny+1;b++) for(int c=0;c<=nz+1;c++) {
			density[a,b,c]=v.Density (gridPt(a,b,c));
		}

		// Initialize indices array
		// First 3 dimensions represent the cell origin position, 4th dimension represents the edge direction (x/y/z)
		int[,,,] indices=new int[nx+1,ny+1,nz+1,3];
		for(int a=0;a<nx+1;a++) for(int b=0;b<ny+1;b++) for(int c=0;c<nz+1;c++) {
			/// Helper function to get the index of a vertex (in the indices array) on the edge 
			/// connecting points referred by (a,b,c) and (a+i,b+j,c+k)
			System.Func<int,int,int,int> getEdgePt=(i,j,k)=> {
				Vector3 p0=gridPt(a,b,c);
				Vector3 p1=gridPt(a+i,b+j,c+k);
				float d0=density[a,b,c];
				float d1=density[a+i,b+j,c+k];

				// If densities are of the same sign, we will never need this edge center point
				if(d0*d1>=0) {
					return -1;
				}

				// Add the edge center to the vertices array and return its index in the array
				vert.Add(p0+d0/(d0-d1)*(p1-p0));
				return vert.Count-1;
			};
			indices[a,b,c,0]=getEdgePt(1,0,0);
			indices[a,b,c,1]=getEdgePt(0,1,0);
			indices[a,b,c,2]=getEdgePt(0,0,1);
		}

		// Marching Cubes implementation
		// Iterate through all cells
		for(int i=0;i<nx;i++) {
			for(int j=0;j<ny;j++) {
				for(int k=0;k<nz;k++) {

					// Get the case -- a binary mask to quickly access the mesh faces for this cell
					int icase=0;
					for(int l=0,t=1;l<8;l++,t*=2) {
						if(density[i+edgeVerts[l*3+0],j+edgeVerts[l*3+1],k+edgeVerts[l*3+2]]>0) icase|=t;
					}

					// Helper function to get the index of an edge center point for this cell
					System.Func<int,int> getEdgeVert=(ed)=> {
						return indices[(i+edgeStart[ed*3+0]),(j+edgeStart[ed*3+1]),(k+edgeStart[ed*3+2]),
							(edgeDir[ed*3+0]+edgeDir[ed*3+1]*2+edgeDir[ed*3+2]*3)-1];
					};

					// Add triangles for this cell
					for(int l=0;l<numPolys[icase];l++) {
						// The polys array contains the indices (with respect to a cell) 
						// of the vertices of all triangles of all cases
						for(int m=0;m<3;m++) {
							// Get the index of the vertex 'm' of the triangle 'l' of the case 'icase'
							int ed=polys[icase*15+l*3+m];
							int iv0=getEdgeVert(ed);
							tri.Add (iv0);
						}
					}
				}
			}
		}

		// Nothing to do. This happens if density is of the same sign everywhere in this volume
		if(tri.Count==0) return null;

		// Set up the mesh
		Mesh mesh=new Mesh();
		mesh.vertices=vert.ToArray ();
		mesh.SetTriangles(tri.ToArray (),0);
		mesh.RecalculateBounds ();mesh.RecalculateNormals ();
		
		return mesh;
	}

	/// <summary>
	/// Gets the LOD. Uses approximate bounds 
	/// </summary>
	/// <returns>The LOD</returns>
	/// <param name="p">Position of the camera/player</param>
	public int getLOD(Vector3 p) {
		// Get an approximate bounds on this part of the terrain
		Vector3 cen=new Vector3(px+0.5f,py+0.5f,pz+0.5f)*v.partSize;
		Bounds b=new Bounds(cen,v.partSize*Vector3.one);

		return getLOD(p, b);
	}

	/// <summary>
	/// Gets the LOD for a specified bounds object. (which can be obtained using mesh.bounds)
	/// </summary>
	/// <returns>The LOD</returns>
	/// <param name="p">Position of the camera/player</param>
	/// <param name="b">The bounds object</param>
	private int getLOD(Vector3 p, Bounds b) {
		// Get an approximate distance of the camera (at position p) from the mesh
		float d=Vector3.Distance(p,b.center)-b.extents.Max ();
		
		// Get the LOD from the LOD list
		// LOD list contains LOD arranged in increasing order of distance
		for(int i=0;i<v.lod.Length;i++) {
			if(d<v.lod[i].maxDis) 
				return i;
		}
		
		return v.lod.Length-1;
	}

	/// <summary>
	/// Loads the meshes according to LOD
	/// </summary>
	/// <param name="p">Position of the camera/player</param>
	public void LoadLODMeshes(Vector3 p) {
		if(_chunks==null) return;
		Bounds b0=new Bounds(cen,v.partSize*Vector3.one);
		foreach(VTChunk c in _chunks) {
			if(c==null)
				continue;
			MeshFilter mf = c.GetComponent<MeshFilter>();
			// Get the LOD
			int l = c.lod = getLOD(p, mf.sharedMesh!=null ? mf.sharedMesh.bounds : b0);

			// Load the mesh for this part at this LOD 
			string path = "VT/LOD" + l + "/" + v.partName (px,py,pz) + "/" + c.gameObject.name + ".asset";
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

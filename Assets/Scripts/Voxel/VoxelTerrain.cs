using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Main class representing the VoxelTerrain
// A VoxelTerrain is divided into (npart^3) number of VTPart objects. Each VTPart has a number of chunks (VTChunk) which contain the actual mesh of that region
// Can be used only as part of the editor scripts in Unity to make a voxel terrain in the editor
// Put this script on any Gameobject. Click on Init button in Inspector to initialize.
// In editor, you can select a part of the terrain by clicking in that region. Then, click on "Make Part" to generate a voxel terrain in that region.
// This plugin generates terrain in parts and chunks, as then an update operation on a region (at runtime, for instance) will cost less, as it will only have to update the mesh specific to that region.
// See also the editor scripts (VTEd.cs) 

public class VoxelTerrain : MonoBehaviour {

	/// <summary>
	/// Size of this voxel terrain in all 3 dimensions 
	/// </summary>
	public float size=1000;

	/// <summary>
	/// Number of parts that the terrain is divided into
	/// </summary>
	public int npart=10;

	/// <summary>
	/// Gets the size of a part.
	/// </summary>
	/// <value>The size of a part.</value>
	public float partSize {
		get {return size/(float)npart;}
	}

	[SerializeField] private VTPart[] _part;
	// Getter and setter for a part
	public VTPart getPart(int i,int j,int k) {
		return _part[i*npart*npart+j*npart+k];
	}
	public void setPart(int i,int j,int k,VTPart p) {
		_part[i*npart*npart+j*npart+k]=p;
	}

	public string partName(int i,int j,int k) {
		return "Part("+i+","+j+","+k+")";
	}

	/// <summary>
	/// The size of the chunk/block of a VTPart. Each chunk contains a separate mesh.
	/// </summary>
	public float blockSize=200;
	[System.Serializable]
	public class LOD {
		// Max Distance from the center of a part of the terrain, that the camera/player can be, to view the part of the terrain at this LOD
		public float maxDis;
		// Distance between 2 grid points on the voxel terrain. Represents the grid resolution
		// Marching cubes is used on this grid.
		public float gridSize;
		public LOD(float maxDis,float gridSize) {
			this.maxDis=maxDis;this.gridSize=gridSize;
		}
	}
	public LOD[] lod=new LOD[] {new LOD(1,64)};
	
	public Material mat;
	// Editor variables - represent currently selected part in inspector
	[HideInInspector] public int edx,edy,edz;

	public bool initialized=false;

	/// <summary>
	/// Initialize the VoxelTerrain and its parts (npart^3 in number).
	/// This is called from the editor script
	/// Can Initialize multiple times - each initialize will reset the terrain.
	/// </summary>
	public void Init () {
		// Set to true to signal editor script
		initialized=true;

		// Initialize part array
		_part=new VTPart[npart*npart*npart];
		for(int i=0;i<transform.childCount;) {
			// Delete all parts
			if(transform.GetChild (i).name.StartsWith ("Part")) DestroyImmediate (transform.GetChild (i).gameObject);
			else i++;
		}

		// Delete previous mesh gameobject
		if(GameObject.Find("VTMesh")!=null)
			DestroyImmediate (GameObject.Find ("VTMesh"));

		// LOD list contains LOD arranged in increasing order of distance
		// Sort LOD list according to distance
		System.Array.Sort(lod,(LOD l0,LOD l1)=> {
			if(l0.maxDis<l1.maxDis) return -1;
			if(l0.maxDis>l1.maxDis) return 1;
			return 0;
		});

		for(int i=0;i<npart;i++) for(int j=0;j<npart;j++) for(int k=0;k<npart;k++) {
			GameObject g=new GameObject(partName(i,j,k));
			VTPart p=g.AddComponent<VTPart>();
			p.Init (this,i,j,k);
			setPart(i,j,k,p);
			g.transform.parent=transform;
		}
	}

	/// <summary>
	/// Loads the meshes for this terrain from disk/memory (using UnityEngine.Resources) according to LOD.
	/// Also makes the transition mesh between different LOD levels, to prevent cracks in the terrain
	/// </summary>
	/// <param name="cp">Position of the camera/player</param>
	public void LoadMeshes(Vector3 cp) {
		foreach(VTPart p in _part) {
			p.LoadLODMeshes (cp);
		}
		foreach(VTPart p in _part) {
			p.MakeTransitions ();
		}
	}
	

	/// <summary>
	/// The frequency for the noise generator
	/// </summary>
	public float noiseFrequency=0.00632f/7;
	/// <summary>
	/// The noise amplitude for the noise generator.
	/// </summary>
	public float noiseAmpRatio=0.09f;
	/// <summary>
	/// Density at the specified position. Currently uses the Simplex Noise generator.
	/// </summary>
	/// <param name="p">Position</param>
	public float Density(Vector3 p) {

		float d=size/4-(p-Vector3.one*size/2).magnitude;
		float f=noiseFrequency,a=size*noiseAmpRatio;
		// Helper functions
		// signed noise at p
		System.Func<float,float,float> NLQs=(mf,ma)=>a*ma*(SimplexNoise.Noise.Generate (p*f*mf));
		// signed noise at specified position
		System.Func<float,float,Vector3,float> NLQs1=(mf,ma,p1)=>a*ma*(SimplexNoise.Noise.Generate (p1*f*mf));
		// unsigned noise at p
		System.Func<float,float,float> NLQu=(mf,ma)=>a*ma*(SimplexNoise.Noise.Generate (p*f*mf)+1)/2;

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
		
		return d;
	}
	
	
}

using UnityEngine;
using System.Collections;

public class VoxelTerrain : MonoBehaviour {
	
	public VTPart[] _part;
	public VTPart part(int i,int j) {
		return _part[i*ny+j];
	}
	public Vector3 size=new Vector3(10000,1000,10000);
	public Vector3 partSize {
		get {return new Vector3(size.x/(float)nx,size.y,size.z/(float)ny);}
	}
	public int nx=10,ny=10;
	[System.Serializable]
	public class LOD {
		public float d,dn;
	}
	public LOD[] lod;
	
	public bool showTrees=false,showVeg=false;
	[HideInInspector] public int edx,edy;
	[HideInInspector] public Material[] mat;
	
	public void Init () {
		_part=new VTPart[nx*ny];
		for(int i=0;i<transform.childCount;) {
			if(transform.GetChild (i).name.StartsWith ("Part")) DestroyImmediate (transform.GetChild (i).gameObject);
			else i++;
		}
		for(int i=0;i<nx;i++) for(int j=0;j<ny;j++) {
			GameObject g=new GameObject("Part("+i+","+j+")");
			VTPart p=_part[i*ny+j]=g.AddComponent<VTPart>();
			p.size=partSize;
			p.mat=mat[Random.Range (0,mat.Length)];
			g.transform.parent=transform;
			g.SetActive (false);
		}
	}
	
}

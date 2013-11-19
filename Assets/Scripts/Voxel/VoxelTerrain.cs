using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelTerrain : MonoBehaviour {
	
	public float size=1000;
	public int npart=10;
	public float partSize {
		get {return size/(float)npart;}
	}
	public VTPart[] _part;
	public VTPart part(int i,int j,int k) {
		return _part[i*npart*npart+j*npart+k];
	}
	public void part(int i,int j,int k,VTPart p) {
		_part[i*npart*npart+j*npart+k]=p;
	}
	public string partName(int i,int j,int k) {
		return "Part("+i+","+j+","+k+")";
	}
	public float blockSize=200;
	[System.Serializable]
	public class LOD {
		public float d,dn;
	}
	public LOD[] lod;
	
	public Material mat;
	[HideInInspector] public int edx,edy,edz;
	
	public void Init () {
		_part=new VTPart[npart*npart*npart];
		for(int i=0;i<transform.childCount;) {
			if(transform.GetChild (i).name.StartsWith ("Part")) DestroyImmediate (transform.GetChild (i).gameObject);
			else i++;
		}
		for(int i=0;i<npart;i++) for(int j=0;j<npart;j++) for(int k=0;k<npart;k++) {
			GameObject g=new GameObject(partName(i,j,k));
			VTPart p=g.AddComponent<VTPart>();
			part(i,j,k,p);p.v=this;
			p.px=i;p.py=j;p.pz=k;
			g.transform.parent=transform;
		}
	}
	
	public void ManageLOD(Vector3 cp) {
		foreach(VTPart p in _part) {
			p.ManageLOD (cp);
		}
		foreach(VTPart p in _part) {
			p.MakeTransitions ();
		}
	}
	
	
	public float freq=0.00632f/7,ampRatio=0.25f;
	public float Density(Vector3 p) {
		/*Vector3 v=p-size/2;v.y=0;
		float h=100+peakHt-v.magnitude/size.magnitude*2*peakHt;
		float d=(h-p.y)*(p.y-100);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.x)-size.x*0.4f,0f,Mathf.Infinity);
		d+=-1e3f*Mathf.Clamp (Mathf.Abs(v.z)-size.z*0.4f,0f,Mathf.Infinity);*/
		float d=size/4-(p-Vector3.one*size/2).magnitude;
		float f=freq,a=size*ampRatio;
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
		
		//float hard_floor_y = size/4;
		//d += Mathf.Clamp01((hard_floor_y - p.y)*3)*40;  
		return d;
	}
	
	
}

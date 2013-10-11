using UnityEngine;
using System.Collections;

public class VTChunk : MonoBehaviour {
	
	public int x,y,z;
	public int lod;
	public VTChunk[] _neigh=new VTChunk[6];
	public VTChunk getNeigh(int i,int j,int k) {
		return _neigh[Mathf.Abs(i)*(i+1)/2+Mathf.Abs(j)*(2+(j+1)/2)+Mathf.Abs(k)*(4+(k+1)/2)];
	}
	public void setNeigh(int i,int j,int k,VTChunk c) {
		_neigh[Mathf.Abs(i)*(i+1)/2+Mathf.Abs(j)*(2+(j+1)/2)+Mathf.Abs(k)*(4+(k+1)/2)]=c;
	}
}

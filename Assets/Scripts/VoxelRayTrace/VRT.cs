using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRT : MonoBehaviour {

	public Mesh Make () {
		List<Vector3> vert=new List<Vector3>();
		List<int> ind=new List<int>();
		Vector3 dx=Vector3.right*10,dy=Vector3.up*10;
		vert.Add (-dx-dy);
		vert.Add (dx-dy);
		vert.Add (dx+dy);
		vert.Add (-dx+dy);
		System.Func<int,int,int,int> ia=(i,j,k)=> {
			ind.Add (i);ind.Add (j);ind.Add (k);return 0;
		};
		ia (0,2,1);
		ia (0,3,2);
		Mesh m=new Mesh();
		m.vertices=vert.ToArray ();
		m.SetIndices (ind.ToArray (),MeshTopology.Triangles,0);
		m.RecalculateBounds ();
		return m;
	}
}

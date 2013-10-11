using UnityEngine;
using System.Collections.Generic;

// Runevision edited

public class Triangulator
{
    public List<Vector2> m_points = new List<Vector2>();
 
	public Triangulator (Vector3[] points,Vector3 normal) {
		Quaternion q=Quaternion.FromToRotation (normal,Vector3.up);
		foreach(Vector3 v in points) {
			Vector3 v1=q*(v-points[0]);
	        m_points.Add(new Vector2(v1.x,v1.z));
		}
	}
    public Triangulator (Vector3[] points,Quaternion q) {
		foreach(Vector3 v in points) {
			Vector3 v1=q*v;
	        m_points.Add(new Vector2(v1.x,v1.z));
		}
    }
	
	public static Mesh getMesh(Vector3[] v) {
		return getMesh (v,Quaternion.identity,null);
	}
	public static Mesh getMesh(Vector3[] v,Quaternion q,Transform par) {
		Mesh m=new Mesh();
		Triangulator t=new Triangulator(v,q);
		int[] ind=t.Triangulate();
		m.vertices=v;m.SetTriangles (ind,0);
		List<Vector2> uv=new List<Vector2>();
		foreach(Vector2 p in t.m_points) {
			if(par!=null) {
				Vector3 pp=par.localToWorldMatrix.MultiplyPoint (new Vector3(p.x,0,p.y));
				uv.Add (new Vector2(pp.x,pp.z));
			} else uv.Add (p);
		}
		m.uv=uv.ToArray ();
		m.RecalculateBounds ();
		m.RecalculateNormals ();
		//m.hideFlags=HideFlags.HideAndDontSave;
		return m;
	}
	
    public int[] Triangulate() {
        List<int> indices = new List<int>();
 
        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();
 
        int[] V = new int[n];
        if (Area() > 0) {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }
 
        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2; ) {
            if ((count--) <= 0)
                return indices.ToArray();
 
            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;
 
            if (Snip(u, v, w, nv, V)) {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }
 
        indices.Reverse();
		/*if(Area ()<0) {
			for(int i=0;i<indices.Count;i+=3) {
				int t=indices[i];indices[i]=indices[i+1];indices[i+1]=t;
			}
		}*/
        return indices.ToArray();
    }
 
    private float Area () {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++) {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }
 
    private bool Snip (int u, int v, int w, int n, int[] V) {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
		float a=(((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x)));
		float eps=1e-5f;//Mathf.Epsilon;
        if (eps > a)
            return false;
        for (p = 0; p < n; p++) {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }
 
    private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;
 
        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;
 
        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;
 
        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
	
	public static float getArea(Vector3[] p) {
		Triangulator t=new Triangulator(p,Quaternion.identity);
		return t.Area ();
	}
	
	public static void quadTris(List<Vector3> v,List<int> tt,int n,int sk,int k0) {
		int k1=n-k0;
		if(k0>k1) {
			for(int k=sk;k<sk+k1-1;k++) {
				tt.Add (k);tt.Add (k+1);tt.Add (2*sk+n-k-1);
				tt.Add (k+1);tt.Add (2*sk+n-k-2);tt.Add (2*sk+n-k-1);
			}
			for(int k=sk+k1-1;k<sk+k0-1;k++) {
				tt.Add (k);tt.Add (k+1);tt.Add (2*sk+n-k1);
			}
		} else {
			for(int k=sk;k<sk+k0-1;k++) {
				tt.Add (k);tt.Add (k+1);tt.Add (2*sk+n-k-1);
				tt.Add (k+1);tt.Add (2*sk+n-k-2);tt.Add (2*sk+n-k-1);
			}
			for(int k=sk+k0-1;k<sk+k1-1;k++) {
				tt.Add (k0-1);tt.Add (2*sk+n-k-2);tt.Add (2*sk+n-k-1);
			}
		}
	}
	
}
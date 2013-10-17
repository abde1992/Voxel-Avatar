using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class UtilMath {
	
	[System.Serializable]
	public class ListVector3 {
		public List<Vector3> l;
		public ListVector3(List<Vector3> tl) {l=tl;}
	}
	
	/*public static IntPoint getTerPt(Vector3 p,TerrainData td) {
		int x=Mathf.RoundToInt (p.x/td.size.x*(float)(td.heightmapWidth-1));
		int y=Mathf.RoundToInt (p.z/td.size.z*(float)(td.heightmapHeight-1));
		return new IntPoint(x,y);
	}
	public static Vector3 getWorldPt(IntPoint p,TerrainData td) {
		float x=(float)p.x*td.size.x/(float)(td.heightmapWidth-1);
		float y=(float)p.y*td.size.z/(float)(td.heightmapHeight-1);
		return new Vector3(x,0,y);
	}
	
	public static bool isConvex(List<Vector3> op,List<Vector3> p,int i) {
		if(p.Count<4) return true;
		List<IntPoint> fp=UtilMath.polyListFromVec (op.ToArray ());
		bool orient=Clipper.Orientation (fp);
		Vector3 p1=p[(i-1+p.Count)%p.Count],p2=p[i],p3=p[(i+1)%p.Count];
		Vector3 up=Vector3.Cross (p3-p2,p2-p1);
		float oy=orient?1:-1;
		if(up.y*oy<=0) return false;
		return true;
	}
	public static int closestEdge(List<Vector3> lp,Vector3 p) {
		int cl=-1;float n=1e4f;
		for(int i=0;i<lp.Count;i++) {
			Vector3 p0=lp[i],p1=lp[(i+1)%lp.Count];
			Vector3 dir=(p1-p0).normalized,dir1=p-p0;
			float dn=(dir1-Vector3.Dot(dir1,dir)*dir).magnitude;
			if(dn<n) {n=dn;cl=i;}
		}return cl;
	}
	
	private static Vector2 polyWorldSize=new Vector2(100,100);private static IntPoint polyGridSize=new IntPoint(10000,10000);
	public static Vector2 polyCellSize=new Vector2(polyWorldSize.x/(float)polyGridSize.x,polyWorldSize.y/(float)polyGridSize.y);
	public static Matrix4x4 polyMat=Matrix4x4.identity;
	public static Quaternion polySetupVerticalPlane(Vector3 p0,Vector3 dir) {
		Quaternion q=Quaternion.AngleAxis (Vector3.Angle (dir,Vector3.right),Vector3.Cross (dir,Vector3.right));
		q=Quaternion.Euler (-90,0,0)*q;
		polyMat=Matrix4x4.TRS (p0,Quaternion.Inverse(q),Vector3.one);
		return q;
	}
	public static IntPoint polyGrid(Vector3 p) {
		p=polyMat.inverse.MultiplyPoint (p);
		long lx=Mathf.RoundToInt (p.x/polyCellSize.x);
		long ly=Mathf.RoundToInt (p.z/polyCellSize.y);
		return new IntPoint(lx,ly);
	}
	public static IntPoint polyGrid(float x,float y) {
		return polyGrid (new Vector3(x,0,y));
	}
	public static Vector3 polyWorld(IntPoint p) {
		float x=polyCellSize.x*(float)p.x;
		float y=polyCellSize.y*(float)p.y;
		Vector3 p1=polyMat.MultiplyPoint (new Vector3(x,0,y));
		return p1;
	}
	public static List<IntPoint> polyListFromVec(Vector3[] p) {
		List<IntPoint> pi=new List<IntPoint>();
		foreach(Vector3 pp in p) pi.Add (polyGrid (pp));
		return pi;
	}
	public static List<Vector3> polyVecFromList(List<IntPoint> pi) {
		List<Vector3> p=new List<Vector3>();
		for(int i=0;i<pi.Count;i++) p.Add (polyWorld (pi[i]));
		return p;
	}
	public static bool polyListFromTree(PolyTree t,out List<List<IntPoint>> p) {
		p=new List<List<IntPoint>>();
		for(int i=0;i<t.ChildCount;i++) {
			PolyNode n=t.Childs[i];
			if(n.IsHole) return false;
			p.Add (n.Contour);
		}
		return true;
	}
	public static void polyClip(Clipper cl,List<IntPoint> p1,List<IntPoint> p2,PolyTree sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygon (p1,PolyType.ptSubject);
		cl.AddPolygon (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<IntPoint> p1,List<IntPoint> p2,List<List<IntPoint>> sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygon (p1,PolyType.ptSubject);
		cl.AddPolygon (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<List<IntPoint>> p1,List<IntPoint> p2,PolyTree sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygons (p1,PolyType.ptSubject);
		cl.AddPolygon (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<List<IntPoint>> p1,List<IntPoint> p2,List<List<IntPoint>> sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygons (p1,PolyType.ptSubject);
		cl.AddPolygon (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<IntLL> p1,List<IntPoint> p2,List<List<IntPoint>> sol,ClipType ct) {
		List<List<IntPoint>> p=new List<List<IntPoint>>();
		foreach(IntLL l in p1) p.AddRange ((List<List<IntPoint>>)l);
		polyClip (cl,p,p2,sol,ct);
	}
	public static void polyClip(Clipper cl,List<IntPoint> p1,List<List<IntPoint>> p2,PolyTree sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygon (p1,PolyType.ptSubject);
		cl.AddPolygons (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<IntPoint> p1,List<List<IntPoint>> p2,List<List<IntPoint>> sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygon (p1,PolyType.ptSubject);
		cl.AddPolygons (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<IntPoint> p1,List<IntLL> p2,List<List<IntPoint>> sol,ClipType ct) {
		List<List<IntPoint>> p=new List<List<IntPoint>>();
		foreach(IntLL l in p2) p.AddRange ((List<List<IntPoint>>)l);
		polyClip (cl,p1,p,sol,ct);
	}
	public static void polyClip(Clipper cl,List<List<IntPoint>> p1,List<List<IntPoint>> p2,PolyTree sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygons (p1,PolyType.ptSubject);
		cl.AddPolygons (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<List<IntPoint>> p1,List<List<IntPoint>> p2,List<List<IntPoint>> sol,ClipType ct) {
		cl.Clear ();
		cl.AddPolygons (p1,PolyType.ptSubject);
		cl.AddPolygons (p2,PolyType.ptClip);
		sol.Clear ();
		cl.Execute (ct,sol);
	}
	public static void polyClip(Clipper cl,List<IntLL> p1,List<IntLL> p2,List<List<IntPoint>> sol,ClipType ct) {
		List<List<IntPoint>> p=new List<List<IntPoint>>();
		List<List<IntPoint>> q=new List<List<IntPoint>>();
		foreach(IntLL l in p1) p.AddRange ((List<List<IntPoint>>)l);
		foreach(IntLL l in p2) q.AddRange ((List<List<IntPoint>>)l);
		polyClip (cl,p,q,sol,ct);
	}
	public static IntRect polyBounds(Clipper cl,List<IntPoint> p) {
		cl.Clear ();cl.AddPolygon (p,PolyType.ptSubject);
		return cl.GetBounds ();
	}
	public static Bounds polyWorldBounds(Clipper cl,List<IntPoint> p) {
		IntRect r=polyBounds (cl,p);
		Vector3 size=new Vector3((float)(r.right-r.left)*polyCellSize.x,0,(float)(r.bottom-r.top)*polyCellSize.y);
		Bounds b=new Bounds(polyWorld (new IntPoint((r.left+r.right)/2,(r.top+r.bottom)/2)),size);
		return b;
	}
	public static Bounds polyWorldBounds(Clipper cl,List<List<IntPoint>> p) {
		Bounds b=polyWorldBounds (cl,p[0]);
		foreach(List<IntPoint> pp in p) b.Encapsulate (polyWorldBounds (cl,pp));
		return b;
	}
	public static bool pointInPoly(Clipper cl,List<IntPoint> poly,IntPoint pi) {
		int err=1;
		IntPoint dx=(IntPoint)Vector2.right*(err);
		IntPoint dy=(IntPoint)Vector2.up*(err);
		List<List<IntPoint>> sol=new List<List<IntPoint>>();
		polyClip (cl,new List<IntPoint>(new IntPoint[] {pi-dx-dy,pi+dx-dy,pi+dx+dy,pi-dx+dy}),poly,sol,ClipType.ctDifference);
		if(sol.Count>0&&sol[0].Count>0) return false;
		return true;
	}
	public static bool polyRemoveSoloSelfInt(List<IntPoint> p,List<IntPoint> p1,List<IntPoint> p2) {
		for(int i=0;i<p.Count;i++) {
			for(int j=i+1;j<p.Count;j++) {
				if((p[j]-p[i]).magnitude<2) {
					p1.AddRange (p.GetRange (0,i+1));
					p1.AddRange (p.GetRange (j+1,p.Count-1-j));
					p2.AddRange (p.GetRange (i+1,j-i));
					return true;
				}
			}
		}
		return false;
	}
	
	public static bool PolyProp(Clipper cl,List<IntPoint> wall,IntRect rect,List<List<IntPoint>> prop,Quaternion q,List<Mesh> mo,List<Mesh> mi) {
		return PolyProp (cl,wall,rect,prop,q,mo,mi,null);
	}
	public static bool PolyProp(Clipper cl,List<IntPoint> wall,IntRect rect,List<List<IntPoint>> prop,Quaternion q,List<Mesh> mo,List<Mesh> mi,Transform par) {
		List<IntPoint> px=new List<IntPoint>();
		foreach(List<IntPoint> lp in prop) {
			foreach(IntPoint p in lp) px.Add (p);
		}
		px.Sort (delegate(IntPoint p1,IntPoint p2) {
			if(p1.x<p2.x) return -1;
			if(p1.x>p2.x) return 1;
			return 0;
		});
		px.Insert (0,new IntPoint(rect.left,0));px.Add (new IntPoint(rect.right,0));
		
		mi.Clear ();mo.Clear ();
		
		for(int j=0;j<px.Count-1;j++) {
			long x0=px[j].x,x1=px[j+1].x;
			List<long> y=new List<long>(new long[] {rect.top,Mathf.Min (px[j].y,px[j+1].y),Mathf.Max (px[j].y,px[j+1].y),rect.bottom});
			for(int k=0;k<y.Count-1;k++) {
				IntRect r1=new IntRect(x0,y[k],x1,y[k+1]);
				List<IntPoint> poly=r1.getVerts ();
				PolyTree sol=new PolyTree();
				List<List<IntPoint>> sol1;
				UtilMath.polyClip (cl,poly,wall,sol,ClipType.ctIntersection);
				if(!UtilMath.polyListFromTree (sol,out sol1)) {Debug.LogError ("hole in intersection");return false;}
				UtilMath.polyClip (cl,sol1,prop,sol,ClipType.ctDifference);
				if(!UtilMath.polyListFromTree (sol,out sol1)) {Debug.LogError ("hole in difference");return false;}
				//sol1=Clipper.SimplifyPolygons (sol1,PolyFillType.pftEvenOdd);
				foreach(List<IntPoint> s in sol1) {
					List<List<IntPoint>> ls=new List<List<IntPoint>>();
					List<IntPoint> s1=new List<IntPoint>(),s2=new List<IntPoint>();
					if(UtilMath.polyRemoveSoloSelfInt (s,s1,s2)) {
						ls.Add (s1);ls.Add (s2);
					} else ls.Add (s);
					foreach(List<IntPoint> ss in ls) {
						List<Vector3> p=UtilMath.polyVecFromList (ss);
						mi.Add(Triangulator.getMesh(p.ToArray(),q,par));
						p.Reverse ();
						mo.Add(Triangulator.getMesh(p.ToArray(),q,par));
						//MakeFloor (mi,mo,f.transform,"Floor"+j);
					}
				}
			}
		}
		return true;
	}
	*/
	
	public static Vector3 to3(this Vector2 v) {
		return new Vector3(v.x,0,v.y);
	}
	public static Vector2 to2(this Vector3 v) {
		return new Vector2(v.x,v.z);
	}
	
	public static bool checkLineSegmentIntersection(Vector3 p0,Vector3 p1,Vector3 p) {
		Vector3 dir=p1-p0;
		if(Vector3.Distance (p,p0)<0.1f||Vector3.Distance (p,p1)<0.1f) return true;
		float a=Vector3.Angle (p-p0,dir.normalized);
		if(a>1f) return false;
		float d=Vector3.Dot (p-p0,dir.normalized);
		if(d<=dir.magnitude) return true;
		return false;
	}
	public static bool checkLineSegmentIntersection(Vector2 p0,Vector2 p1,Vector2 p) {
		Vector2 dir=p1-p0;
		if(Vector2.Distance (p,p0)<0.1f||Vector2.Distance (p,p1)<0.1f) return true;
		//p0-=dir.normalized*0.1f;
		//p1+=dir.normalized*0.1f;
		float a=Vector2.Angle (p-p0,dir.normalized);
		if(a>1f) return false;
		float d=Vector2.Dot (p-p0,dir.normalized);
		if(d<dir.magnitude) return true;
		return false;
	}
	public class Line {
		public float a,b,c;public Vector2 p,dir;
		public Line(Vector2 p,Vector2 dir) {
			dir.Normalize ();
			a=-dir.y;b=dir.x;
			c=-p.x*dir.y+p.y*dir.x;
			this.p=p;this.dir=dir;
		}
		public Line(Vector3 p,Vector3 dir) {
			dir.Normalize ();
			a=-dir.z;b=dir.x;
			c=-p.x*dir.z+p.z*dir.x;
			this.p=p.to2();
			this.dir=dir.to2();
		}
	}
	public static Vector2 getLineIntersection(Line l1,Line l2) {
		float d=l1.a*l2.b-l1.b*l2.a;
		if(Mathf.Abs (d)<1e-4f) {
			if(l1.a*l2.c-l1.c*l2.a<1e-4f) return l1.p;
			return Vector2.zero;
		}
		Vector2 p=new Vector2();
		p.x=(l1.c*l2.b-l1.b*l2.c)/d;
		p.y=(l1.a*l2.c-l1.c*l2.a)/d;
		return p;
	}
	public static bool getLineSegmentIntersection(Vector2 p0,Vector2 p1,Vector2 q0,Vector2 q1,out Vector2 ip) {
		ip=Vector2.zero;
		Line l1=new Line(p0,(p1-p0).normalized),l2=new Line(q0,(q1-q0).normalized);
		float d=l1.a*l2.b-l1.b*l2.a;
		if(Mathf.Abs (d)<1e-4f) {
			if(checkLineSegmentIntersection (p0,p1,q0)||checkLineSegmentIntersection (p0,p1,q1)||
				checkLineSegmentIntersection (q0,q1,p0)||checkLineSegmentIntersection (q0,q1,p1))
			{ip=p0;return true;}
		} else {
			Vector2 p=getLineIntersection (l1,l2);
			if(checkLineSegmentIntersection (p0,p1,p)&&checkLineSegmentIntersection (q0,q1,p)) {ip=p;return true;}
		}
		return false;
	}
	
	/*public static bool getLineSegmentIntersection(Vector3 p0,Vector3 p1,Vector3 q0,Vector3 q1,out Vector3 ip) {
		ip=Vector3.zero;
		Vector2 ip2;
		if(getLineSegmentIntersection (UtilMath.polyGrid (p0),UtilMath.polyGrid (p1),UtilMath.polyGrid (q0),UtilMath.polyGrid(q1),out ip2)) {
			ip=UtilMath.polyWorld ((IntPoint)ip2);return true;
		}return false;
	}*/
	
	public static float DiscretizeDir(Vector3 d0,Vector3 d1,float err,bool half) {
		d1.y=0;d0.y=0;d1.Normalize ();d0.Normalize ();
		float ang=Vector3.Angle (d0,d1);
		float adir=0;
		if(ang>err) {
			Vector3 dr=Vector3.Cross (Vector3.up,d0);
			float dot=Vector3.Dot (d1,dr);
			if(!half&&Vector3.Angle(-d0,d1)<err) adir=-1;
			else if(dot>0) adir=(half?1:0.5f);
			else adir=(half?-1:-0.5f);
		} else adir=0;
		return adir;
	}
	
	public static Quaternion Lerp(Quaternion q1,Quaternion q2,float wsp) {
		float ang=Quaternion.Angle (q1,q2);
		return Quaternion.Slerp (q1,q2,wsp*Time.deltaTime/ang);
	}
	
	public static Vector3 Cross(this Vector2 p,Vector2 p1) {
		return new Vector3(0,p.x*p1.y-p.y*p1.x,0);
	}
	public static Vector3 Abs(this Vector3 p) {
		return new Vector3(Mathf.Abs (p.x),Mathf.Abs (p.y),Mathf.Abs (p.z));
	}
	public static Vector3 SetY(this Vector3 p,float y) {
		Vector3 p1=p;
		p1.y=y;return p1;
	}
	public static Vector3 GetX(this Vector3 p) {
		return Vector3.right*p.x;
	}
	public static Vector3 GetY(this Vector3 p) {
		return Vector3.up*p.y;
	}
	public static Vector3 GetZ(this Vector3 p) {
		return Vector3.forward*p.z;
	}
	public static float Max(this Vector3 p) {
		return Mathf.Max (p.x,Mathf.Max(p.y,p.z));
	}
	public static Vector3 Mul(this Vector3 p,Vector3 p1) {
		return new Vector3(p.x*p1.x,p.y*p1.y,p.z*p1.z);
	}
	
	public static int singleLayer(this LayerMask l) {
		for(int i=0;i<32;i++) {
			if(l/(1<<i)==1) return i;
		}return -1;
	}
	
	public static void Write(this BinaryWriter bw,Vector3 p) {
		bw.Write (p.x);bw.Write (p.y);bw.Write (p.z);
	}
	public static Vector3 ReadVector3(this BinaryReader br) {
		return new Vector3(br.ReadSingle (),br.ReadSingle (),br.ReadSingle ());
	}
	
	public static List<Vector3s> toSArray(this List<Vector3> p) {
		List<Vector3s> v=new List<Vector3s>();
		foreach(Vector3 pp in p) v.Add ((Vector3s)pp);
		return v;
	}
	public static List<Vector3> toVArray(this List<Vector3s> p) {
		List<Vector3> v=new List<Vector3>();
		foreach(Vector3s pp in p) v.Add ((Vector3)pp);
		return v;
	}
	public static List<Vector3> toVArray(this Vector3s[] p) {
		return new List<Vector3s>(p).toVArray();
	}
	public static List<Vector3> DeserializeVec3s(this System.Xml.Serialization.XmlSerializer xs,FileStream fs) {
		List<Vector3> v=new List<Vector3>();
		List<Vector3s> p=((List<Vector3s>)xs.Deserialize (fs));
		foreach(Vector3s pp in p) v.Add ((Vector3)pp);
		return v;
	}
	[System.Serializable]
	public class Vector3s {
		public float x,y,z;
		public Vector3s(Vector3 v) {
			x=v.x;y=v.y;z=v.z;
		}
		public Vector3s() {}
		public static implicit operator Vector3s (Vector3 v) {
			return new Vector3s(v);
		}
		public static explicit operator Vector3 (Vector3s v) {
			return new Vector3(v.x,v.y,v.z);
		}
	}
	
	
	
}
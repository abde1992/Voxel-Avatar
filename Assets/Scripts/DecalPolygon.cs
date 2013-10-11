/*
Copyright (c) 2010, Raphael Lopes Baldi & Aquiris Game Experience LTDA.

See the document "TERMS OF USE" included in the project folder for licencing details.
*/

using UnityEngine;
using System.Collections.Generic;

public class DecalPolygon
{
	public int verticeCount;
	public Vector3[] normal;
	public Vector3[] vertice;
	public Vector4[] tangent;
	public Vector2[] uv;
	
	public DecalPolygon()
	{
		verticeCount = 0;
		vertice = new Vector3[9];
		normal = new Vector3[9];
		tangent = new Vector4[9];
		uv=new Vector2[9];
	}
	public DecalPolygon(Vector3 a,Vector3 b,Vector3 c) {
		verticeCount=3;
		Vector3 n=Vector3.Cross(c-b,a-b).normalized;
		Vector3 t=(a-b).normalized;
		Vector2 u=Vector2.zero;
		vertice=new Vector3[] {a,b,c};
		normal=new Vector3[] {n,n,n};
		tangent=new Vector4[] {t,t,t};
		uv=new Vector2[] {u,u,u};
	}
	
	static public DecalPolygon ClipPolygonAgainstPlane (DecalPolygon polygon, Vector4 plane, bool pos=true)
	{
		bool[] neg = new bool[10];
		int negCount = 0;
		
		Vector3 n = new Vector3(plane.x, plane.y, plane.z);

		for(int i = 0; i < polygon.verticeCount; i++)
		{
			neg[i] = (Vector3.Dot(polygon.vertice[i], n) + plane.w) < 0.0f;
			if(!pos) neg[i]=!neg[i];
			if(neg[i]) negCount++;
		}
		
		if(negCount == polygon.verticeCount) return null;
		if(negCount == 0) return polygon;

		DecalPolygon tempPolygon = new DecalPolygon();
		tempPolygon.verticeCount = 0;
		Vector3 v1, v2, dir;
		float t;
		
		for(int i = 0; i < polygon.verticeCount; i++)
		{
			int b = (i == 0) ? polygon.verticeCount - 1 : i -1;
			
			if(neg[i])
			{
				if(!neg[b])
				{
					v1 = polygon.vertice[i];
					v2 = polygon.vertice[b];
					dir = (v2 - v1).normalized;
					float d=(v2-v1).magnitude;
				
					t = -(Vector3.Dot(n, v1) + plane.w) / Vector3.Dot(n, dir);
					
					tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[i] + ((polygon.tangent[b] - polygon.tangent[i]).normalized * t);
					tempPolygon.vertice[tempPolygon.verticeCount] = v1 + ((v2 - v1).normalized * t);
					tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[i] + ((polygon.normal[b] - polygon.normal[i]).normalized * t);
					tempPolygon.uv[tempPolygon.verticeCount] = polygon.uv[i] + ((polygon.uv[b] - polygon.uv[i]) * t/d);
					
					tempPolygon.verticeCount++;
				}
			}
			else
			{
				if(neg[b])
				{
					v1 = polygon.vertice[b];
					v2 = polygon.vertice[i];
					dir = (v2 - v1).normalized;
					float d=(v2-v1).magnitude;
					
					t = -(Vector3.Dot(n, v1) + plane.w) / Vector3.Dot(n, dir);
					
					tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[b] + ((polygon.tangent[i] - polygon.tangent[b]).normalized * t);
					tempPolygon.vertice[tempPolygon.verticeCount] = v1 + ((v2 - v1).normalized * t);
					tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[b] + ((polygon.normal[i] - polygon.normal[b]).normalized * t);
					tempPolygon.uv[tempPolygon.verticeCount] = polygon.uv[b] + ((polygon.uv[i] - polygon.uv[b]) * t/d);
					
					tempPolygon.verticeCount++;
				}
				
				tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[i];
				tempPolygon.vertice[tempPolygon.verticeCount] = polygon.vertice[i];
				tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[i];
				tempPolygon.uv[tempPolygon.verticeCount] = polygon.uv[i];
				
				tempPolygon.verticeCount++;
			}
		}
		
		return tempPolygon;
	}
	
	public void get(out Vector3[] vert,out int[] trt,out Vector2[] tuv) {
		Vector3 normal=Vector3.Cross(vertice[2]-vertice[1],vertice[0]-vertice[1]);
		vert=new List<Vector3>(vertice).GetRange (0,verticeCount).ToArray ();
		Triangulator t=new Triangulator(vert,normal);
		trt=t.Triangulate ();
		tuv=new List<Vector2>(uv).GetRange (0,verticeCount).ToArray();
	}
}
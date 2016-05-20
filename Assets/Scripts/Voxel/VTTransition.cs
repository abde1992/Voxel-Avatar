using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The following code uses Eric Lengyel's Transvoxel Algorithm.
// http://transvoxel.org/
// Lengyel, Eric. “Voxel-Based Terrain for Real-Time Virtual Simulations”. PhD diss., University of California at Davis, 2010.

// TODO: Create a smoother transition mesh, to remove lighting artifacts
public partial class VTPart {

	/// <summary>
	/// Makes a mesh bridging the transitions between different LOD meshes.
	/// To remove cracks in the final mesh
	/// We expect the difference in LOD to be atmost 1
	/// </summary>
	public void MakeTransitions() {

		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();

		foreach(VTChunk c in _chunks) {
			if(c==null) continue;
			Vector3 or=origin+v.blockSize*new Vector3(c.x,c.y,c.z),size=v.blockSize*Vector3.one;
			// Get no. of grid points for this chunk
			int nx=Mathf.RoundToInt (size.x/v.lod[c.lod].gridSize);
			int ny=Mathf.RoundToInt (size.y/v.lod[c.lod].gridSize);
			int nz=Mathf.RoundToInt (size.z/v.lod[c.lod].gridSize);

			// If there is a neighbor in the y-direction with a different LOD
			if(c.getNeigh (0,1,0)!=null&&c.lod==c.getNeigh (0,1,0).lod+1) {
				// Fix the faces normal to y-direction
				for(int i=0;i<nx;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(i+(float)a/2)/(float)nx*size.x,size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						// Get the case
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];

						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									// Density values should never be of the same sign
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
			
			if(c.getNeigh (0,-1,0)!=null&&c.lod==c.getNeigh (0,-1,0).lod+1) {
				for(int i=0;i<nx;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(i+(float)a/2)/(float)nx*size.x,(float)(0)/(float)ny*size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
			
			if(c.getNeigh (1,0,0)!=null&&c.lod==c.getNeigh (1,0,0).lod+1) {
				for(int i=0;i<ny;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(nx)/(float)nx*size.x,(float)(i+(float)a/2)/(float)ny*size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							//Vector3 nor=Vector3.Cross (vert[vert.Count-2]-vert[vert.Count-3],vert[vert.Count-1]-vert[vert.Count-2]).normalized;
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
			
			if(c.getNeigh (-1,0,0)!=null&&c.lod==c.getNeigh (-1,0,0).lod+1) {
				for(int i=0;i<ny;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(0)/(float)nx*size.x,(float)(i+(float)a/2)/(float)ny*size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							//Vector3 nor=Vector3.Cross (vert[vert.Count-2]-vert[vert.Count-3],vert[vert.Count-1]-vert[vert.Count-2]).normalized;
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
			
			if(c.getNeigh (0,0,1)!=null&&c.lod==c.getNeigh (0,0,1).lod+1) {
				for(int i=0;i<ny;i++) {
					for(int j=0;j<nx;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(j+(float)b/2)/(float)nx*size.x,(float)(i+(float)a/2)/(float)ny*size.y,(float)(nz)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
			
			if(c.getNeigh (0,0,-1)!=null&&c.lod==c.getNeigh (0,0,-1).lod+1) {
				for(int i=0;i<ny;i++) {
					for(int j=0;j<nx;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(j+(float)b/2)/(float)nx*size.x,(float)(i+(float)a/2)/(float)ny*size.y,(float)(0)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(v.Density (gridPt(a,b))<0)
								icase+=transitionCaseValues[b*3+a];
						int classIndex=transitionCellClass[icase];
						bool invert=(classIndex&0x80)>0;
						classIndex=classIndex&0x7F;
						int gc=transitionCellData[classIndex][0];
						int nv=gc>>4,tc=gc&0x0F;
						for(int a=0;a<tc*3;a+=3) {
							for(int b=0;b<3;b++) {
								int vd=transitionVertexData[icase][transitionCellData[classIndex][a+1+b]];
								int iv0=(vd&0x0F),iv1=(vd&0xFF)>>4;
								Vector3 p0=gridPt(transitionVertexPos[iv0][0],transitionVertexPos[iv0][1]);
								Vector3 p1=gridPt(transitionVertexPos[iv1][0],transitionVertexPos[iv1][1]);
								float d0=v.Density(p0);
								float d1=v.Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
						}
					}
				}
			}
		}

		Mesh transitionMesh=new Mesh();
		transitionMesh.vertices=vert.ToArray ();
		transitionMesh.SetTriangles (tri.ToArray (),0);
		transitionMesh.RecalculateBounds ();transitionMesh.RecalculateNormals ();
		transitionMesh.hideFlags=HideFlags.DontSave;

		GameObject tr=UtilEditor.createOrGetGO ("Tr",transform);
		if(tr.GetComponent<MeshFilter>()==null) tr.AddComponent<MeshFilter>();
		if(tr.GetComponent<MeshRenderer>()==null) tr.AddComponent<MeshRenderer>();
		tr.GetComponent<MeshFilter>().sharedMesh=transitionMesh;
		tr.GetComponent<MeshRenderer>().sharedMaterial=v.mat;

	}
}

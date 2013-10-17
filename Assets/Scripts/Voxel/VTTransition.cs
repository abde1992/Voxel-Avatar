using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class VTPart {

	public void MakeTransitions() {
		int x=px,y=py;
		List<Vector3> vert=new List<Vector3>();
		List<int> tri=new List<int>();
		foreach(VTChunk c in chunks) {
			Vector3 or=origin+v.blockSize*new Vector3(c.x,c.y,c.z),size=v.blockSize*Vector3.one;
			int nx=Mathf.RoundToInt (size.x/v.lod[c.lod].dn);
			int ny=Mathf.RoundToInt (size.y/v.lod[c.lod].dn);
			int nz=Mathf.RoundToInt (size.z/v.lod[c.lod].dn);
			if(c.getNeigh (0,1,0)!=null&&c.lod==c.getNeigh (0,1,0).lod+1) {
				for(int i=0;i<nx;i++) {
					for(int j=0;j<nz;j++) {
						System.Func<int,int,Vector3> gridPt=(a,b)=> {
							return or+new Vector3((float)(i+(float)a/2)/(float)nx*size.x,(float)(ny)/(float)ny*size.y,(float)(j+(float)b/2)/(float)nz*size.z);
						};
						int icase=0;
						for(int a=0;a<3;a++) for(int b=0;b<3;b++)
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							//Vector3 nor=Vector3.Cross (vert[vert.Count-2]-vert[vert.Count-3],vert[vert.Count-1]-vert[vert.Count-2]).normalized;
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							//Vector3 nor=Vector3.Cross (vert[vert.Count-2]-vert[vert.Count-3],vert[vert.Count-1]-vert[vert.Count-2]).normalized;
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
							if(Density (gridPt(a,b))<0)
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
								float d0=Density(p0);
								float d1=Density(p1);
								if(d0*d1>=0) {
									Debug.LogError ("transition density sign");
								}
								vert.Add(p0+d0/(d0-d1)*(p1-p0));
							}
							for(int b=0,n=vert.Count;b<3;b++) vert.Add (vert[n-3+b]);
							//if(invert) {
								tri.Add (vert.Count-2-3);tri.Add (vert.Count-3-3);tri.Add (vert.Count-1-3);
							//} else {
								tri.Add (vert.Count-3);tri.Add (vert.Count-2);tri.Add (vert.Count-1);
							//}
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
		tr.GetComponent<MeshRenderer>().sharedMaterial=mat;
	}
}
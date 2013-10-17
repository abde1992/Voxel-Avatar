Shader "Custom/VTMakeTex" 
{
  Properties 
	{
		_SpriteTex ("Base (RGB)", 2D) = "white" {}
		_CTNTex ("CTN", 2D) = "white" {}
		_ECLTex ("ECL", 2D) = "white" {}
		//_Size ("Size", Range(0, 3)) = 0.5 
		_Strength ("_Strength", Range(1, 1000)) = 10
	}
 
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 xbox360 gles
				#pragma target 3.0
				#pragma debug
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc"  
				
				#define F3 1.0/3.0 
				#define G3 1.0/6.0 
				
 
				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct GS_INPUT
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float2  tex0	: TEXCOORD0;
				};
 
				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float3  normal  : NORMAL;
				};
 
 
				// **************************************************************
				// Vars															*
				// **************************************************************
 
				float _Strength;
				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;
				Texture2D _CTNTex;
				SamplerState sampler_CTNTex;
				Texture2D _ECLTex;
				SamplerState sampler_ECLTex;
 
				// **************************************************************
				// Shader Programs												*
				// **************************************************************
 
				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(appdata_base v)
				{
					GS_INPUT output = (GS_INPUT)0;
					
					output.pos =  v.vertex; //mul(_Object2World, v.vertex ); 
					//float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);
					//output.pos = mul(vp, output.pos);
					output.normal = v.normal;
					output.tex0 = float2(0, 0);
 
					return output;
				}
				
				float SampleDensity( float4 p  ){
					
					float d=0.1-p.y;
					return d;
				}
      			
      			
				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(15)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)	{
					
					const int2 edge_to_verts[12] = {
						int2(0,1), //0
						int2(1,2), //1
						int2(2,3), //2
						int2(3,0), //3
						int2(4,5), //4
						int2(5,6), //5
						int2(6,7), //6
						int2(7,4), //7
						int2(4,0), //8
						int2(5,1), //9
						int2(6,2), //10
						int2(7,3) //11
					};
					
					const float cap = 0;
					const float halfSize = 0.5;
	 				const float4 cubeVerts[8] = {
						//front face
						float4(-halfSize, -halfSize, -halfSize, 0) ,		//LB   0
						float4(-halfSize,  halfSize, -halfSize,	0) ,		//LT   1
						float4( halfSize,  halfSize, -halfSize, 0) ,		//RT   2
						float4( halfSize, -halfSize, -halfSize, 0) ,		//RB   3
						//back
						float4(-halfSize, -halfSize,  halfSize, 0),		// LB  4
						float4(-halfSize,  halfSize,  halfSize, 0),		// LT  5
						float4( halfSize,  halfSize,  halfSize, 0),		// RT  6
						float4( halfSize, -halfSize,  halfSize, 0)		// RB  7
					};
					
					const float weights[8] = {
						SampleDensity(p[0].pos + cubeVerts[0]),
						SampleDensity(p[0].pos + cubeVerts[1]),
						SampleDensity(p[0].pos + cubeVerts[2]),
						SampleDensity(p[0].pos + cubeVerts[3]),
						SampleDensity(p[0].pos + cubeVerts[4]),
						SampleDensity(p[0].pos + cubeVerts[5]),
						SampleDensity(p[0].pos + cubeVerts[6]),
						SampleDensity(p[0].pos + cubeVerts[7])
					};
 
					int marchingCase = 
					(weights[7] > cap) * 128 + 
					(weights[6] > cap) * 64 +
					(weights[5] > cap) * 32 +
					(weights[4] > cap) * 16 +
					(weights[3] > cap) * 8 +
					(weights[2] > cap) * 4 +
					(weights[1] > cap) * 2 +
					(weights[0] > cap) * 1;
					
					int numpolys = _CTNTex.Sample(sampler_CTNTex, float2(marchingCase/256.0,0));
					if(numpolys==0) return;
					int4 lpolyEdges[] = {
						_ECLTex.Sample(sampler_ECLTex, float2((marchingCase * 5 + 0)/1280.0,0)),
					}
					float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);
					//float4x4 vp = UNITY_MATRIX_IDENT;
					FS_INPUT pIn;
					for( int i = 0; i < numpolys; i++ ){
							
							int4 polyEdges = lpolyEdges[i];
							
							int va = edge_to_verts[polyEdges.x].x;
							int vb = edge_to_verts[polyEdges.x].y;
							float amount = (cap - weights[va]) / (weights[vb] - weights[va]);
							float4 worldPos = lerp( p[0].pos + cubeVerts[va],  p[0].pos + cubeVerts[vb], amount);
							float4 pA = mul(vp, worldPos);
							
							va = edge_to_verts[polyEdges.y].x;
							vb = edge_to_verts[polyEdges.y].y;
							amount = (cap - weights[va]) / (weights[vb] - weights[va]);
							worldPos = lerp( p[0].pos + cubeVerts[va],  p[0].pos + cubeVerts[vb], amount);
							float4 pB = mul(vp, worldPos);
							
							va = edge_to_verts[polyEdges.z].x;
							vb = edge_to_verts[polyEdges.z].y;
							amount = (cap - weights[va]) / (weights[vb] - weights[va]);
							worldPos = lerp( p[0].pos + cubeVerts[va],  p[0].pos + cubeVerts[vb], amount);
							float4 pC = mul(vp, worldPos);
 
							float4 r = pA - pC;
							float4 f = pA - pB;
							float3 normal = normalize(cross(f,r));
 
							pIn.pos = pA;
							pIn.tex0 = float2(1.0f, 0.0f);
							pIn.normal = normal;
							triStream.Append(pIn);
						
							pIn.pos = pB;
							pIn.tex0 = float2(1.0f, 0.0f);
							pIn.normal = normal;
							triStream.Append(pIn);
 
							pIn.normal = normal;
							pIn.pos = pC;
							pIn.tex0 = float2(1.0f, 0.0f);
							triStream.Append(pIn);
 
							triStream.RestartStrip();
					}
					
				}
 
 
 
				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
					return _SpriteTex.Sample(sampler_SpriteTex, input.tex0);//  * saturate(0.5 + input.normal.y * 0.5) ;
				}
 
			ENDCG
		}
	} 
}
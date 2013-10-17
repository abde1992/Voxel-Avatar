Shader "Custom/VTMake" 
{
  Properties 
	{
		_SpriteTex ("Base (RGB)", 2D) = "white" {}
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
					const int case_to_numpolys[256] = {
						0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,2,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,3,
						1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,3,2,3,3,2,3,4,4,3,3,4,4,3,4,5,5,2,
						1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,3,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,4,
						2,3,3,4,3,4,2,3,3,4,4,5,4,5,3,2,3,4,4,3,4,5,3,2,4,5,5,4,5,2,4,1, 
						1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,3,2,3,3,4,3,4,4,5,3,2,4,3,4,3,5,2, 
						2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,4,3,4,4,3,4,5,5,4,4,3,5,2,5,4,2,1, 
						2,3,3,4,3,4,4,5,3,4,4,5,2,3,3,2,3,4,4,5,4,5,5,2,4,3,5,4,3,2,4,1, 
						3,4,4,5,4,5,3,4,4,5,5,2,3,4,2,1,2,3,3,2,3,4,2,1,3,2,4,1,2,1,1,0
					};
					//   256*5 = 1280 entries
					const int4 edge_connect_list[1280] = { 
						int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  8,  3, -1),  int4(9,  8,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1),  int4(1,  2, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  2, 10, -1),  int4(0,  2,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  8,  3, -1),  int4(2, 10,  8, -1), int4(10,  9,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3, 11,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0, 11,  2, -1),  int4(8, 11,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  9,  0, -1),  int4(2,  3, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1, 11,  2, -1),  int4(1,  9, 11, -1),  int4(9,  8, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3, 10,  1, -1), int4(11, 10,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0, 10,  1, -1),  int4(0,  8, 10, -1),  int4(8, 11, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  9,  0, -1),  int4(3, 11,  9, -1), int4(11, 10,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  8, 10, -1), int4(10,  8, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  7,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  3,  0, -1),  int4(7,  3,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1),  int4(8,  4,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  1,  9, -1),  int4(4,  7,  1, -1),  int4(7,  3,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1),  int4(8,  4,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  4,  7, -1),  int4(3,  0,  4, -1),  int4(1,  2, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  2, 10, -1),  int4(9,  0,  2, -1),  int4(8,  4,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2, 10,  9, -1),  int4(2,  9,  7, -1),  int4(2,  7,  3, -1),  int4(7,  9,  4, -1), int4(-1, -1, -1, -1),
						int4(8,  4,  7, -1),  int4(3, 11,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  4,  7, -1), int4(11,  2,  4, -1),  int4(2,  0,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  0,  1, -1),  int4(8,  4,  7, -1),  int4(2,  3, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  7, 11, -1),  int4(9,  4, 11, -1),  int4(9, 11,  2, -1),  int4(9,  2,  1, -1), int4(-1, -1, -1, -1),
						int4(3, 10,  1, -1),  int4(3, 11, 10, -1),  int4(7,  8,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1, 11, 10, -1),  int4(1,  4, 11, -1),  int4(1,  0,  4, -1),  int4(7, 11,  4, -1), int4(-1, -1, -1, -1),
						int4(4,  7,  8, -1),  int4(9,  0, 11, -1),  int4(9, 11, 10, -1), int4(11,  0,  3, -1), int4(-1, -1, -1, -1),
						int4(4,  7, 11, -1),  int4(4, 11,  9, -1),  int4(9, 11, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  4, -1),  int4(0,  8,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  5,  4, -1),  int4(1,  5,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  5,  4, -1),  int4(8,  3,  5, -1),  int4(3,  1,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1),  int4(9,  5,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  0,  8, -1),  int4(1,  2, 10, -1),  int4(4,  9,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5,  2, 10, -1),  int4(5,  4,  2, -1),  int4(4,  0,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2, 10,  5, -1),  int4(3,  2,  5, -1),  int4(3,  5,  4, -1),  int4(3,  4,  8, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  4, -1),  int4(2,  3, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0, 11,  2, -1),  int4(0,  8, 11, -1),  int4(4,  9,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  5,  4, -1),  int4(0,  1,  5, -1),  int4(2,  3, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  1,  5, -1),  int4(2,  5,  8, -1),  int4(2,  8, 11, -1),  int4(4,  8,  5, -1), int4(-1, -1, -1, -1),
						int4(10,  3, 11, -1), int4(10,  1,  3, -1),  int4(9,  5,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  9,  5, -1),  int4(0,  8,  1, -1),  int4(8, 10,  1, -1),  int4(8, 11, 10, -1), int4(-1, -1, -1, -1),
						int4(5,  4,  0, -1),  int4(5,  0, 11, -1),  int4(5, 11, 10, -1), int4(11,  0,  3, -1), int4(-1, -1, -1, -1),
						int4(5,  4,  8, -1),  int4(5,  8, 10, -1), int4(10,  8, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  7,  8, -1),  int4(5,  7,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  3,  0, -1),  int4(9,  5,  3, -1),  int4(5,  7,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  7,  8, -1),  int4(0,  1,  7, -1),  int4(1,  5,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  5,  3, -1),  int4(3,  5,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  7,  8, -1),  int4(9,  5,  7, -1), int4(10,  1,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  1,  2, -1),  int4(9,  5,  0, -1),  int4(5,  3,  0, -1),  int4(5,  7,  3, -1), int4(-1, -1, -1, -1),
						int4(8,  0,  2, -1),  int4(8,  2,  5, -1),  int4(8,  5,  7, -1), int4(10,  5,  2, -1), int4(-1, -1, -1, -1),
						int4(2, 10,  5, -1),  int4(2,  5,  3, -1),  int4(3,  5,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7,  9,  5, -1),  int4(7,  8,  9, -1),  int4(3, 11,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  7, -1),  int4(9,  7,  2, -1),  int4(9,  2,  0, -1),  int4(2,  7, 11, -1), int4(-1, -1, -1, -1),
						int4(2,  3, 11, -1),  int4(0,  1,  8, -1),  int4(1,  7,  8, -1),  int4(1,  5,  7, -1), int4(-1, -1, -1, -1),
						int4(11,  2,  1, -1), int4(11,  1,  7, -1),  int4(7,  1,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  8, -1),  int4(8,  5,  7, -1), int4(10,  1,  3, -1), int4(10,  3, 11, -1), int4(-1, -1, -1, -1),
						int4(5,  7,  0, -1),  int4(5,  0,  9, -1),  int4(7, 11,  0, -1),  int4(1,  0, 10, -1), int4(11, 10,  0, -1),
						int4(11, 10,  0, -1), int4(11,  0,  3, -1), int4(10,  5,  0, -1),  int4(8,  0,  7, -1),  int4(5,  7,  0, -1),
						int4(11, 10,  5, -1),  int4(7, 11,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  6,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1),  int4(5, 10,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  0,  1, -1),  int4(5, 10,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  8,  3, -1),  int4(1,  9,  8, -1),  int4(5, 10,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  6,  5, -1),  int4(2,  6,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  6,  5, -1),  int4(1,  2,  6, -1),  int4(3,  0,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  6,  5, -1),  int4(9,  0,  6, -1),  int4(0,  2,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5,  9,  8, -1),  int4(5,  8,  2, -1),  int4(5,  2,  6, -1),  int4(3,  2,  8, -1), int4(-1, -1, -1, -1),
						int4(2,  3, 11, -1), int4(10,  6,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  0,  8, -1), int4(11,  2,  0, -1), int4(10,  6,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1),  int4(2,  3, 11, -1),  int4(5, 10,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5, 10,  6, -1),  int4(1,  9,  2, -1),  int4(9, 11,  2, -1),  int4(9,  8, 11, -1), int4(-1, -1, -1, -1),
						int4(6,  3, 11, -1),  int4(6,  5,  3, -1),  int4(5,  1,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8, 11, -1),  int4(0, 11,  5, -1),  int4(0,  5,  1, -1),  int4(5, 11,  6, -1), int4(-1, -1, -1, -1),
						int4(3, 11,  6, -1),  int4(0,  3,  6, -1),  int4(0,  6,  5, -1),  int4(0,  5,  9, -1), int4(-1, -1, -1, -1),
						int4(6,  5,  9, -1),  int4(6,  9, 11, -1), int4(11,  9,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5, 10,  6, -1),  int4(4,  7,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  3,  0, -1),  int4(4,  7,  3, -1),  int4(6,  5, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  9,  0, -1),  int4(5, 10,  6, -1),  int4(8,  4,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  6,  5, -1),  int4(1,  9,  7, -1),  int4(1,  7,  3, -1),  int4(7,  9,  4, -1), int4(-1, -1, -1, -1),
						int4(6,  1,  2, -1),  int4(6,  5,  1, -1),  int4(4,  7,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2,  5, -1),  int4(5,  2,  6, -1),  int4(3,  0,  4, -1),  int4(3,  4,  7, -1), int4(-1, -1, -1, -1),
						int4(8,  4,  7, -1),  int4(9,  0,  5, -1),  int4(0,  6,  5, -1),  int4(0,  2,  6, -1), int4(-1, -1, -1, -1),
						int4(7,  3,  9, -1),  int4(7,  9,  4, -1),  int4(3,  2,  9, -1),  int4(5,  9,  6, -1),  int4(2,  6,  9, -1),
						int4(3, 11,  2, -1),  int4(7,  8,  4, -1), int4(10,  6,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5, 10,  6, -1),  int4(4,  7,  2, -1),  int4(4,  2,  0, -1),  int4(2,  7, 11, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1),  int4(4,  7,  8, -1),  int4(2,  3, 11, -1),  int4(5, 10,  6, -1), int4(-1, -1, -1, -1),
						int4(9,  2,  1, -1),  int4(9, 11,  2, -1),  int4(9,  4, 11, -1),  int4(7, 11,  4, -1),  int4(5, 10,  6, -1),
						int4(8,  4,  7, -1),  int4(3, 11,  5, -1),  int4(3,  5,  1, -1),  int4(5, 11,  6, -1), int4(-1, -1, -1, -1),
						int4(5,  1, 11, -1),  int4(5, 11,  6, -1),  int4(1,  0, 11, -1),  int4(7, 11,  4, -1),  int4(0,  4, 11, -1),
						int4(0,  5,  9, -1),  int4(0,  6,  5, -1),  int4(0,  3,  6, -1), int4(11,  6,  3, -1),  int4(8,  4,  7, -1),
						int4(6,  5,  9, -1),  int4(6,  9, 11, -1),  int4(4,  7,  9, -1),  int4(7, 11,  9, -1), int4(-1, -1, -1, -1),
						int4(10,  4,  9, -1),  int4(6,  4, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4, 10,  6, -1),  int4(4,  9, 10, -1),  int4(0,  8,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  0,  1, -1), int4(10,  6,  0, -1),  int4(6,  4,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  3,  1, -1),  int4(8,  1,  6, -1),  int4(8,  6,  4, -1),  int4(6,  1, 10, -1), int4(-1, -1, -1, -1),
						int4(1,  4,  9, -1),  int4(1,  2,  4, -1),  int4(2,  6,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  0,  8, -1),  int4(1,  2,  9, -1),  int4(2,  4,  9, -1),  int4(2,  6,  4, -1), int4(-1, -1, -1, -1),
						int4(0,  2,  4, -1),  int4(4,  2,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  3,  2, -1),  int4(8,  2,  4, -1),  int4(4,  2,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  4,  9, -1), int4(10,  6,  4, -1), int4(11,  2,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  2, -1),  int4(2,  8, 11, -1),  int4(4,  9, 10, -1),  int4(4, 10,  6, -1), int4(-1, -1, -1, -1),
						int4(3, 11,  2, -1),  int4(0,  1,  6, -1),  int4(0,  6,  4, -1),  int4(6,  1, 10, -1), int4(-1, -1, -1, -1),
						int4(6,  4,  1, -1),  int4(6,  1, 10, -1),  int4(4,  8,  1, -1),  int4(2,  1, 11, -1),  int4(8, 11,  1, -1),
						int4(9,  6,  4, -1),  int4(9,  3,  6, -1),  int4(9,  1,  3, -1), int4(11,  6,  3, -1), int4(-1, -1, -1, -1),
						int4(8, 11,  1, -1),  int4(8,  1,  0, -1), int4(11,  6,  1, -1),  int4(9,  1,  4, -1),  int4(6,  4,  1, -1),
						int4(3, 11,  6, -1),  int4(3,  6,  0, -1),  int4(0,  6,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(6,  4,  8, -1), int4(11,  6,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7, 10,  6, -1),  int4(7,  8, 10, -1),  int4(8,  9, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  7,  3, -1),  int4(0, 10,  7, -1),  int4(0,  9, 10, -1),  int4(6,  7, 10, -1), int4(-1, -1, -1, -1),
						int4(10,  6,  7, -1),  int4(1, 10,  7, -1),  int4(1,  7,  8, -1),  int4(1,  8,  0, -1), int4(-1, -1, -1, -1),
						int4(10,  6,  7, -1), int4(10,  7,  1, -1),  int4(1,  7,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2,  6, -1),  int4(1,  6,  8, -1),  int4(1,  8,  9, -1),  int4(8,  6,  7, -1), int4(-1, -1, -1, -1),
						int4(2,  6,  9, -1),  int4(2,  9,  1, -1),  int4(6,  7,  9, -1),  int4(0,  9,  3, -1),  int4(7,  3,  9, -1),
						int4(7,  8,  0, -1),  int4(7,  0,  6, -1),  int4(6,  0,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7,  3,  2, -1),  int4(6,  7,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  3, 11, -1), int4(10,  6,  8, -1), int4(10,  8,  9, -1),  int4(8,  6,  7, -1), int4(-1, -1, -1, -1),
						int4(2,  0,  7, -1),  int4(2,  7, 11, -1),  int4(0,  9,  7, -1),  int4(6,  7, 10, -1),  int4(9, 10,  7, -1),
						int4(1,  8,  0, -1),  int4(1,  7,  8, -1),  int4(1, 10,  7, -1),  int4(6,  7, 10, -1),  int4(2,  3, 11, -1),
						int4(11,  2,  1, -1), int4(11,  1,  7, -1), int4(10,  6,  1, -1),  int4(6,  7,  1, -1), int4(-1, -1, -1, -1),
						int4(8,  9,  6, -1),  int4(8,  6,  7, -1),  int4(9,  1,  6, -1), int4(11,  6,  3, -1),  int4(1,  3,  6, -1),
						int4(0,  9,  1, -1), int4(11,  6,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7,  8,  0, -1),  int4(7,  0,  6, -1),  int4(3, 11,  0, -1), int4(11,  6,  0, -1), int4(-1, -1, -1, -1),
						int4(7, 11,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7,  6, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  0,  8, -1), int4(11,  7,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1), int4(11,  7,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  1,  9, -1),  int4(8,  3,  1, -1), int4(11,  7,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  1,  2, -1),  int4(6, 11,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1),  int4(3,  0,  8, -1),  int4(6, 11,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  9,  0, -1),  int4(2, 10,  9, -1),  int4(6, 11,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(6, 11,  7, -1),  int4(2, 10,  3, -1), int4(10,  8,  3, -1), int4(10,  9,  8, -1), int4(-1, -1, -1, -1),
						int4(7,  2,  3, -1),  int4(6,  2,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(7,  0,  8, -1),  int4(7,  6,  0, -1),  int4(6,  2,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  7,  6, -1),  int4(2,  3,  7, -1),  int4(0,  1,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  6,  2, -1),  int4(1,  8,  6, -1),  int4(1,  9,  8, -1),  int4(8,  7,  6, -1), int4(-1, -1, -1, -1),
						int4(10,  7,  6, -1), int4(10,  1,  7, -1),  int4(1,  3,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  7,  6, -1),  int4(1,  7, 10, -1),  int4(1,  8,  7, -1),  int4(1,  0,  8, -1), int4(-1, -1, -1, -1),
						int4(0,  3,  7, -1),  int4(0,  7, 10, -1),  int4(0, 10,  9, -1),  int4(6, 10,  7, -1), int4(-1, -1, -1, -1),
						int4(7,  6, 10, -1),  int4(7, 10,  8, -1),  int4(8, 10,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(6,  8,  4, -1), int4(11,  8,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  6, 11, -1),  int4(3,  0,  6, -1),  int4(0,  4,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  6, 11, -1),  int4(8,  4,  6, -1),  int4(9,  0,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  4,  6, -1),  int4(9,  6,  3, -1),  int4(9,  3,  1, -1), int4(11,  3,  6, -1), int4(-1, -1, -1, -1),
						int4(6,  8,  4, -1),  int4(6, 11,  8, -1),  int4(2, 10,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1),  int4(3,  0, 11, -1),  int4(0,  6, 11, -1),  int4(0,  4,  6, -1), int4(-1, -1, -1, -1),
						int4(4, 11,  8, -1),  int4(4,  6, 11, -1),  int4(0,  2,  9, -1),  int4(2, 10,  9, -1), int4(-1, -1, -1, -1),
						int4(10,  9,  3, -1), int4(10,  3,  2, -1),  int4(9,  4,  3, -1), int4(11,  3,  6, -1),  int4(4,  6,  3, -1),
						int4(8,  2,  3, -1),  int4(8,  4,  2, -1),  int4(4,  6,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  4,  2, -1),  int4(4,  6,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  9,  0, -1),  int4(2,  3,  4, -1),  int4(2,  4,  6, -1),  int4(4,  3,  8, -1), int4(-1, -1, -1, -1),
						int4(1,  9,  4, -1),  int4(1,  4,  2, -1),  int4(2,  4,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  1,  3, -1),  int4(8,  6,  1, -1),  int4(8,  4,  6, -1),  int4(6, 10,  1, -1), int4(-1, -1, -1, -1),
						int4(10,  1,  0, -1), int4(10,  0,  6, -1),  int4(6,  0,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  6,  3, -1),  int4(4,  3,  8, -1),  int4(6, 10,  3, -1),  int4(0,  3,  9, -1), int4(10,  9,  3, -1),
						int4(10,  9,  4, -1),  int4(6, 10,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  9,  5, -1),  int4(7,  6, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1),  int4(4,  9,  5, -1), int4(11,  7,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5,  0,  1, -1),  int4(5,  4,  0, -1),  int4(7,  6, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  7,  6, -1),  int4(8,  3,  4, -1),  int4(3,  5,  4, -1),  int4(3,  1,  5, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  4, -1), int4(10,  1,  2, -1),  int4(7,  6, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(6, 11,  7, -1),  int4(1,  2, 10, -1),  int4(0,  8,  3, -1),  int4(4,  9,  5, -1), int4(-1, -1, -1, -1),
						int4(7,  6, 11, -1),  int4(5,  4, 10, -1),  int4(4,  2, 10, -1),  int4(4,  0,  2, -1), int4(-1, -1, -1, -1),
						int4(3,  4,  8, -1),  int4(3,  5,  4, -1),  int4(3,  2,  5, -1), int4(10,  5,  2, -1), int4(11,  7,  6, -1),
						int4(7,  2,  3, -1),  int4(7,  6,  2, -1),  int4(5,  4,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  4, -1),  int4(0,  8,  6, -1),  int4(0,  6,  2, -1),  int4(6,  8,  7, -1), int4(-1, -1, -1, -1),
						int4(3,  6,  2, -1),  int4(3,  7,  6, -1),  int4(1,  5,  0, -1),  int4(5,  4,  0, -1), int4(-1, -1, -1, -1),
						int4(6,  2,  8, -1),  int4(6,  8,  7, -1),  int4(2,  1,  8, -1),  int4(4,  8,  5, -1),  int4(1,  5,  8, -1),
						int4(9,  5,  4, -1), int4(10,  1,  6, -1),  int4(1,  7,  6, -1),  int4(1,  3,  7, -1), int4(-1, -1, -1, -1),
						int4(1,  6, 10, -1),  int4(1,  7,  6, -1),  int4(1,  0,  7, -1),  int4(8,  7,  0, -1),  int4(9,  5,  4, -1),
						int4(4,  0, 10, -1),  int4(4, 10,  5, -1),  int4(0,  3, 10, -1),  int4(6, 10,  7, -1),  int4(3,  7, 10, -1),
						int4(7,  6, 10, -1),  int4(7, 10,  8, -1),  int4(5,  4, 10, -1),  int4(4,  8, 10, -1), int4(-1, -1, -1, -1),
						int4(6,  9,  5, -1),  int4(6, 11,  9, -1), int4(11,  8,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  6, 11, -1),  int4(0,  6,  3, -1),  int4(0,  5,  6, -1),  int4(0,  9,  5, -1), int4(-1, -1, -1, -1),
						int4(0, 11,  8, -1),  int4(0,  5, 11, -1),  int4(0,  1,  5, -1),  int4(5,  6, 11, -1), int4(-1, -1, -1, -1),
						int4(6, 11,  3, -1),  int4(6,  3,  5, -1),  int4(5,  3,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 10, -1),  int4(9,  5, 11, -1),  int4(9, 11,  8, -1), int4(11,  5,  6, -1), int4(-1, -1, -1, -1),
						int4(0, 11,  3, -1),  int4(0,  6, 11, -1),  int4(0,  9,  6, -1),  int4(5,  6,  9, -1),  int4(1,  2, 10, -1),
						int4(11,  8,  5, -1), int4(11,  5,  6, -1),  int4(8,  0,  5, -1), int4(10,  5,  2, -1),  int4(0,  2,  5, -1),
						int4(6, 11,  3, -1),  int4(6,  3,  5, -1),  int4(2, 10,  3, -1), int4(10,  5,  3, -1), int4(-1, -1, -1, -1),
						int4(5,  8,  9, -1),  int4(5,  2,  8, -1),  int4(5,  6,  2, -1),  int4(3,  8,  2, -1), int4(-1, -1, -1, -1),
						int4(9,  5,  6, -1),  int4(9,  6,  0, -1),  int4(0,  6,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  5,  8, -1),  int4(1,  8,  0, -1),  int4(5,  6,  8, -1),  int4(3,  8,  2, -1),  int4(6,  2,  8, -1),
						int4(1,  5,  6, -1),  int4(2,  1,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  3,  6, -1),  int4(1,  6, 10, -1),  int4(3,  8,  6, -1),  int4(5,  6,  9, -1),  int4(8,  9,  6, -1),
						int4(10,  1,  0, -1), int4(10,  0,  6, -1),  int4(9,  5,  0, -1),  int4(5,  6,  0, -1), int4(-1, -1, -1, -1),
						int4(0,  3,  8, -1),  int4(5,  6, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  5,  6, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  5, 10, -1),  int4(7,  5, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  5, 10, -1), int4(11,  7,  5, -1),  int4(8,  3,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5, 11,  7, -1),  int4(5, 10, 11, -1),  int4(1,  9,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(10,  7,  5, -1), int4(10, 11,  7, -1),  int4(9,  8,  1, -1),  int4(8,  3,  1, -1), int4(-1, -1, -1, -1),
						int4(11,  1,  2, -1), int4(11,  7,  1, -1),  int4(7,  5,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1),  int4(1,  2,  7, -1),  int4(1,  7,  5, -1),  int4(7,  2, 11, -1), int4(-1, -1, -1, -1),
						int4(9,  7,  5, -1),  int4(9,  2,  7, -1),  int4(9,  0,  2, -1),  int4(2, 11,  7, -1), int4(-1, -1, -1, -1),
						int4(7,  5,  2, -1),  int4(7,  2, 11, -1),  int4(5,  9,  2, -1),  int4(3,  2,  8, -1),  int4(9,  8,  2, -1),
						int4(2,  5, 10, -1),  int4(2,  3,  5, -1),  int4(3,  7,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  2,  0, -1),  int4(8,  5,  2, -1),  int4(8,  7,  5, -1), int4(10,  2,  5, -1), int4(-1, -1, -1, -1),
						int4(9,  0,  1, -1),  int4(5, 10,  3, -1),  int4(5,  3,  7, -1),  int4(3, 10,  2, -1), int4(-1, -1, -1, -1),
						int4(9,  8,  2, -1),  int4(9,  2,  1, -1),  int4(8,  7,  2, -1), int4(10,  2,  5, -1),  int4(7,  5,  2, -1),
						int4(1,  3,  5, -1),  int4(3,  7,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  7, -1),  int4(0,  7,  1, -1),  int4(1,  7,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  0,  3, -1),  int4(9,  3,  5, -1),  int4(5,  3,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9,  8,  7, -1),  int4(5,  9,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5,  8,  4, -1),  int4(5, 10,  8, -1), int4(10, 11,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(5,  0,  4, -1),  int4(5, 11,  0, -1),  int4(5, 10, 11, -1), int4(11,  3,  0, -1), int4(-1, -1, -1, -1),
						int4(0,  1,  9, -1),  int4(8,  4, 10, -1),  int4(8, 10, 11, -1), int4(10,  4,  5, -1), int4(-1, -1, -1, -1),
						int4(10, 11,  4, -1), int4(10,  4,  5, -1), int4(11,  3,  4, -1),  int4(9,  4,  1, -1),  int4(3,  1,  4, -1),
						int4(2,  5,  1, -1),  int4(2,  8,  5, -1),  int4(2, 11,  8, -1),  int4(4,  5,  8, -1), int4(-1, -1, -1, -1),
						int4(0,  4, 11, -1),  int4(0, 11,  3, -1),  int4(4,  5, 11, -1),  int4(2, 11,  1, -1),  int4(5,  1, 11, -1),
						int4(0,  2,  5, -1),  int4(0,  5,  9, -1),  int4(2, 11,  5, -1),  int4(4,  5,  8, -1), int4(11,  8,  5, -1),
						int4(9,  4,  5, -1),  int4(2, 11,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  5, 10, -1),  int4(3,  5,  2, -1),  int4(3,  4,  5, -1),  int4(3,  8,  4, -1), int4(-1, -1, -1, -1),
						int4(5, 10,  2, -1),  int4(5,  2,  4, -1),  int4(4,  2,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3, 10,  2, -1),  int4(3,  5, 10, -1),  int4(3,  8,  5, -1),  int4(4,  5,  8, -1),  int4(0,  1,  9, -1),
						int4(5, 10,  2, -1),  int4(5,  2,  4, -1),  int4(1,  9,  2, -1),  int4(9,  4,  2, -1), int4(-1, -1, -1, -1),
						int4(8,  4,  5, -1),  int4(8,  5,  3, -1),  int4(3,  5,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  4,  5, -1),  int4(1,  0,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(8,  4,  5, -1),  int4(8,  5,  3, -1),  int4(9,  0,  5, -1),  int4(0,  3,  5, -1), int4(-1, -1, -1, -1),
						int4(9,  4,  5, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4, 11,  7, -1),  int4(4,  9, 11, -1),  int4(9, 10, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  8,  3, -1),  int4(4,  9,  7, -1),  int4(9, 11,  7, -1),  int4(9, 10, 11, -1), int4(-1, -1, -1, -1),
						int4(1, 10, 11, -1),  int4(1, 11,  4, -1),  int4(1,  4,  0, -1),  int4(7,  4, 11, -1), int4(-1, -1, -1, -1),
						int4(3,  1,  4, -1),  int4(3,  4,  8, -1),  int4(1, 10,  4, -1),  int4(7,  4, 11, -1), int4(10, 11,  4, -1),
						int4(4, 11,  7, -1),  int4(9, 11,  4, -1),  int4(9,  2, 11, -1),  int4(9,  1,  2, -1), int4(-1, -1, -1, -1),
						int4(9,  7,  4, -1),  int4(9, 11,  7, -1),  int4(9,  1, 11, -1),  int4(2, 11,  1, -1),  int4(0,  8,  3, -1),
						int4(11,  7,  4, -1), int4(11,  4,  2, -1),  int4(2,  4,  0, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(11,  7,  4, -1), int4(11,  4,  2, -1),  int4(8,  3,  4, -1),  int4(3,  2,  4, -1), int4(-1, -1, -1, -1),
						int4(2,  9, 10, -1),  int4(2,  7,  9, -1),  int4(2,  3,  7, -1),  int4(7,  4,  9, -1), int4(-1, -1, -1, -1),
						int4(9, 10,  7, -1),  int4(9,  7,  4, -1), int4(10,  2,  7, -1),  int4(8,  7,  0, -1),  int4(2,  0,  7, -1),
						int4(3,  7, 10, -1),  int4(3, 10,  2, -1),  int4(7,  4, 10, -1),  int4(1, 10,  0, -1),  int4(4,  0, 10, -1),
						int4(1, 10,  2, -1),  int4(8,  7,  4, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  9,  1, -1),  int4(4,  1,  7, -1),  int4(7,  1,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  9,  1, -1),  int4(4,  1,  7, -1),  int4(0,  8,  1, -1),  int4(8,  7,  1, -1), int4(-1, -1, -1, -1),
						int4(4,  0,  3, -1),  int4(7,  4,  3, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(4,  8,  7, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9, 10,  8, -1), int4(10, 11,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  0,  9, -1),  int4(3,  9, 11, -1), int4(11,  9, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  1, 10, -1),  int4(0, 10,  8, -1),  int4(8, 10, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  1, 10, -1), int4(11,  3, 10, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  2, 11, -1),  int4(1, 11,  9, -1),  int4(9, 11,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  0,  9, -1),  int4(3,  9, 11, -1),  int4(1,  2,  9, -1),  int4(2, 11,  9, -1), int4(-1, -1, -1, -1),
						int4(0,  2, 11, -1),  int4(8,  0, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(3,  2, 11, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  3,  8, -1),  int4(2,  8, 10, -1), int4(10,  8,  9, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(9, 10,  2, -1),  int4(0,  9,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(2,  3,  8, -1),  int4(2,  8, 10, -1),  int4(0,  1,  8, -1),  int4(1, 10,  8, -1), int4(-1, -1, -1, -1),
						int4(1, 10,  2, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(1,  3,  8, -1),  int4(9,  1,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  9,  1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(0,  3,  8, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1),
						int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1), int4(-1, -1, -1, -1)
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
					
					int numpolys = case_to_numpolys[marchingCase];
					if(numpolys==0) return;
				
					float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);
					//float4x4 vp = UNITY_MATRIX_IDENT;
					FS_INPUT pIn;
					for( int i = 0; i < 5; i++ ){
						if(i >= numpolys){
							break;
							pIn.pos = float4(0,0,0,0) + p[0].pos;
							pIn.tex0 = float2(1.0f, 0.0f);
							pIn.normal = float3(0,1,0);
							triStream.Append(pIn);
							triStream.Append(pIn);
							triStream.Append(pIn);
							triStream.RestartStrip();
						}else{
							
							int4 polyEdges = edge_connect_list[marchingCase * 5 + i];
							
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
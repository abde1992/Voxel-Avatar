Shader "Custom/WavingGrass" {
   Properties {
		_WavingTint ("Fade Color", Color) = (.7,.6,.5, 0)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
		_Cutoff ("Cutoff", float) = 0.5
	}
	CGINCLUDE
#include "UnityCG.cginc"
#include "TerrainEngine.cginc"
#pragma glsl_no_auto_normalization
ENDCG

		
   SubShader {
      Tags {
			"Queue" = "Geometry+200"
			"IgnoreProjector"="True"
			"RenderType"="GrassBillboard"
		}
		Cull Off
		LOD 200
		ColorMask RGB
	
         CGPROGRAM
         
         #pragma surface surf Lambert vertex:vert addshadow
		 #pragma exclude_renderers flash
 
         // User-specified uniforms            
         uniform sampler2D _MainTex;        
		fixed _Cutoff;
 
 
         void vert(inout appdata_full input) 
         {
 			float4 dir=input.tangent-input.vertex;
 			//dir.z=0;
 			float3 nor=normalize(input.normal);
         	float3 dirv=normalize(mul(UNITY_MATRIX_MV, float4(nor.xyz,0)).xyz);
         	float4 dir0=dir;
         	
         	{
	 			float c=dot(nor,float3(1,0,0));
	 			float s=sin(acos(c));
	 			float2 dir1=normalize(dir.xy);
	 			float2 dir2=float2(dir1.x*c-dir1.y*s,dir1.x*s+dir1.y*c)*length(dir.xy);
	 			//dir.xy=dir2.xy;
 			}
 			//dir=lerp(dir,dir0,dirv.z);
 			
 			//dir=mul(transpose(UNITY_MATRIX_IT_MV), dir);
 			
            input.vertex = input.tangent-dir;
              //+mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              //- float4(-dir.x, -dir.z, 0.0, 1.0)
 
         }
         
         struct Input {
			float2 uv_MainTex;
			half3 worldNormal;
			fixed4 color : COLOR;
		};
		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			fixed4 d = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = d.a;
			float4 nor=float4(IN.worldNormal.xyz,0);
			nor=mul(UNITY_MATRIX_MV,mul(_World2Object,nor));
			//if(abs(nor.z)<0.6) o.Alpha=0;
			clip (o.Alpha - _Cutoff);
			//o.Alpha *= IN.color.a;
		}
 
 
         ENDCG
   }
}
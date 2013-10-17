Shader "Custom/VRT" {
	Properties {
		_NoiseTex ("Noise", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert 
			#pragma fragment frag 
			#include "UnityCG.cginc"
	
			sampler2D _NoiseTex;
	
			struct FS_Input {
				float4 v:POSITION;
				float3 scr:TEXCOORD0;
				float3 wp:TEXCOORD1;
				float2 depth:TEXCOORD2;
			};
	
			FS_Input vert (appdata_base input) {
				FS_Input f;
				f.v=mul(UNITY_MATRIX_MVP,input.vertex);
				f.scr=f.v.xyz;
				f.scr/=f.v.w;
				f.wp=mul(_Object2World,input.vertex);
				f.depth.r=f.v.z;
				return f;
			}
			
			float Density(float3 pos) {
				float d=-pos.y;
				d+=tex2D(_NoiseTex,pos.xy).r;
				return d;
			}
			
			void frag(FS_Input f,out float4 col:COLOR,out float depth:DEPTH) {
				col=float4(1,1,1,1);
				float td=100;
				int n=100;
				float3 cp=_WorldSpaceCameraPos;
				float3 dir=normalize(f.wp-cp);
				float dn=td/n,d=0,pdn,cdn;
				depth=1;
				for(int i=0;i<n;i++) {
					float3 p=f.wp+dir*d;
					if(cdn=Density(p)>=0) {
						//d=d-dn+pdn/(pdn-cdn)*(dn);
						//float4 pp=float4(p.xyz,1);
						//pp=mul(UNITY_MATRIX_MVP,mul(_World2Object,pp));
						depth=-1+(f.depth+d-_ProjectionParams.y)/(_ProjectionParams.z-_ProjectionParams.y)*2;
						break;
					}
					pdn=cdn;
					d+=dn;
				}
			}
			
			ENDCG
		}
	}
}

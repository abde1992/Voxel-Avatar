Shader "Custom/VT" {
	Properties {
		_Splat0 ("Layer 0 (RGB)", 2D) = "white" {}
		_Splat1 ("Layer 1 (RGB)", 2D) = "white" {}
		_Splat2 ("Layer 2 (RGB)", 2D) = "white" {}
		_Splat3 ("Layer 3 (RGB)", 2D) = "white" {}
		_Detail ("Detail (RGB)", 2D) = "gray" {}
		_Splat0Ht ("Splat0 Ht", Float) = 0.8
		_Splat1Ht ("Splat1 Ht", Float) = 0.6
		_Splat2Ht ("Splat2 Ht", Float) = 0.4
		_Splat3Ht ("Splat3 Ht", Float) = 0
		_DS ("Detail Scale", Float) = 0.1
	}
	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType"="Opaque" 
		}
		LOD 200
		
		CGPROGRAM
		#pragma target 3.0
		//#pragma surface surf Lambert
		#pragma surface surf WrapLambert

      half4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = dot (s.Normal, lightDir);
          half diff = NdotL * 0.5 + 0.5;
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
          c.a = s.Alpha;
          return c;
      }

		
		sampler2D _Splat3;
		sampler2D _Splat2;
		sampler2D _Splat1;
		sampler2D _Splat0;
		sampler2D _Detail;
		float _Splat0Ht;
		float _Splat1Ht;
		float _Splat2Ht;
		float _Splat3Ht;
		float _DS;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		half2 coord1,coord2,coord3;half3 blend_weights;
		half4 blendTex(in sampler2D tex) {
			half4 col1 = tex2D (tex,coord1);  
			half4 col2 = tex2D (tex,coord2);  
			half4 col3 = tex2D (tex,coord3);  
			return col1.xyzw * blend_weights.x+col2.xyzw * blend_weights.y+col3.xyzw * blend_weights.z;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			
			blend_weights = abs( IN.worldNormal.xyz );
			//if(abs(blend_weights.y)<0.1) blend_weights.y=0.1;
			// Tighten up the blending zone:  
			blend_weights = (blend_weights - 0.2) * 7;  
			blend_weights = max(blend_weights, 0);
			// Force weights to sum to 1.0 (very important!)  
			blend_weights /= (blend_weights.x + blend_weights.y + blend_weights.z ).xxx;   
			coord1 = IN.worldPos.yz*1;  
			coord2 = IN.worldPos.zx*1;  
			coord3 = IN.worldPos.xy*1;  
			
			float4 splat_weights=float4(0,0,0,0);
			float y=abs(IN.worldNormal.y);
			float y0=y-_Splat0Ht;
			float y1=y-_Splat1Ht;
			float y2=y-_Splat2Ht;
			float y3=y-_Splat3Ht;
			float d0=1-_Splat0Ht;
			float d1=_Splat0Ht-_Splat1Ht;
			float d2=_Splat1Ht-_Splat2Ht;
			float d3=_Splat2Ht-_Splat3Ht;
			if(y0>=0) {splat_weights.x=lerp(0,1,y0/d0);splat_weights.y=lerp(0,1,1-y0/d0);}
			else if(y1>=0) {splat_weights.y=lerp(0,1,y1/d1);splat_weights.z=lerp(0,1,1-y1/d1);}
			else if(y2>=0) {splat_weights.z=lerp(0,1,y2/d2);splat_weights.w=lerp(0,1,1-y2/d2);}
			else if(y3>=0) splat_weights.w=1;
			
			float4 blended_color; // .w hold spec value  
			blended_color=splat_weights.x*blendTex(_Splat0);
			blended_color+=splat_weights.y*blendTex(_Splat1);
			blended_color+=splat_weights.z*blendTex(_Splat2);
			blended_color+=splat_weights.w*blendTex(_Splat3);
			float ds=_DS;
			coord1*=ds;
			coord2*=ds;
			coord3*=ds;
			//blended_color.rgb *= blendTex(_Detail).rgb;
			
			o.Albedo = blended_color.rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

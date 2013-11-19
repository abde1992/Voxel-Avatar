Shader "Custom/Ast" {
	Properties {
		_MainTex ("Main Texture (RGB)", 2D) = "gray" {}
		sc ("Scale", Float) = 1
		_Detail ("Detail (RGB)", 2D) = "gray" {}
		_DS ("Detail Scale", Float) = 0.1
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	}
	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType"="Opaque" 
		}
		LOD 200
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf WrapLambert

      half4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = dot (s.Normal, lightDir);
          half diff = NdotL * 0.5 + 0.5;
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
          c.a = s.Alpha;
          return c;
      }

		
		float sc;
		sampler2D _MainTex;
		sampler2D _Detail;
		float _DS;
		half _Shininess;

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
			coord1 = IN.worldPos.yz*sc;  
			coord2 = IN.worldPos.zx*sc;  
			coord3 = IN.worldPos.xy*sc;  
			
			float4 blended_color; // .w hold spec value  
			blended_color=blendTex(_MainTex);
			float ds=_DS;
			coord1*=ds;
			coord2*=ds;
			coord3*=ds;
			blended_color.rgb *= blendTex(_Detail).rgb;
			
			o.Albedo = blended_color.rgb;
			o.Specular = _Shininess;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

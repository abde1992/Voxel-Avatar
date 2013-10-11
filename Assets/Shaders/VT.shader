Shader "Custom/VT" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
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

		
		sampler2D _MainTex;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			float3 blend_weights = abs( IN.worldNormal.xyz );   // Tighten up the blending zone:  
			blend_weights = (blend_weights - 0.2) * 7;  
			blend_weights = max(blend_weights, 0);      // Force weights to sum to 1.0 (very important!)  
			blend_weights /= (blend_weights.x + blend_weights.y + blend_weights.z ).xxx;   
			// Now determine a color value and bump vector for each of the 3  
			// projections, blend them, and store blended results in these two  
			// vectors:  
			float4 blended_color; // .w hold spec value  
			// Compute the UV coords for each of the 3 planar projections.  
			// tex_scale (default ~ 1.0) determines how big the textures appear.  
			float ts=0.1;
			float2 coord1 = IN.worldPos.yz*ts;  
			float2 coord2 = IN.worldPos.zx*ts;  
			float2 coord3 = IN.worldPos.xy*ts;  
			// This is where you would apply conditional displacement mapping.  
			//if (blend_weights.x > 0) coord1 = . . .  
			//if (blend_weights.y > 0) coord2 = . . .  
			//if (blend_weights.z > 0) coord3 = . . .  
			// Sample color maps for each projection, at those UV coords.  
			float4 col1 = tex2D (_MainTex,coord1);  
			float4 col2 = tex2D (_MainTex,coord2);  
			float4 col3 = tex2D (_MainTex,coord3);  
			 // Finally, blend the results of the 3 planar projections.  
			blended_color = col1.xyzw * blend_weights.x+col2.xyzw * blend_weights.y+col3.xyzw * blend_weights.z;
			o.Albedo = blended_color.rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

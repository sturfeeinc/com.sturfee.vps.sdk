// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Test/UVTransform" {

	Properties {
		_MainTex ("Base (RGB)", 			2D) 			= "white" {}
		_TexScale ("Scale of Tex", 			Float)			= 1.0
		_TexRatio ("Ratio of Tex", 			Float)			= 1.0
		_Theta 	("Rotation of Tex",			Float)			= 0.0
		
		_PlaneScale ("Scale of Plane Mesh", 		Vector) 		= (1, 1, 0, 0)
	}
	
	SubShader {
	
		LOD 200
		
		CGINCLUDE
		
		#include "UnityCG.cginc"
		
		#pragma target 3.0
		
		sampler2D _MainTex;
		float _TexScale;
		float _TexRatio; // _MainTex.width / _MainTex.height
		float _Theta;
		
		float2 _PlaneScale;
		
		struct v2f {
            		float4 pos : POSITION;
            		float2 texcoord : TEXCOORD0;
        	};
       
	        float3x3 getXYTranslationMatrix (float2 translation) {
        		return float3x3(1, 0, translation.x, 0, 1, translation.y, 0, 0, 1);
        	}
        
	        float3x3 getXYRotationMatrix (float theta) {
	        	float s = -sin(theta);
			float c = cos(theta);
			return float3x3(c, -s, 0, s, c, 0, 0, 0, 1);
	        }
        
	        float3x3 getXYScaleMatrix (float2 scale) {
			return float3x3(scale.x, 0, 0, 0, scale.y, 0, 0, 0, 1);
	        } 
        
	        float2 applyMatrix (float3x3 m, float2 uv) {
	        	return mul(m, float3(uv.x, uv.y, 1)).xy;
	        }
       
	        v2f vert (appdata_full v) {
	        	v2f o;
	        	float2 offset = float2((1 - _PlaneScale.x) * 0.5, (1 - _PlaneScale.y) * 0.5);
                        o.texcoord = applyMatrix(
                        	getXYTranslationMatrix(float2(0.5, 0.5)), 
                        	applyMatrix( // scale
                        		getXYScaleMatrix(float2(1 / _TexRatio, 1) * _TexScale),
                        		applyMatrix( // rotate
                        			getXYRotationMatrix(_Theta), 
                        			applyMatrix(
                        				getXYTranslationMatrix(float2(-0.5, -0.5) + offset),
                        				(v.texcoord.xy * _PlaneScale)
                        			)
                        		)
                        	)
                        );
            
                        o.pos = UnityObjectToClipPos(v.vertex);
                        return o;
                }
    

		half4 frag (v2f IN) : COLOR {
        		return tex2D(_MainTex, IN.texcoord);
		}
		
		ENDCG
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
		
			ENDCG
		}

	} 
	
	FallBack "Diffuse"
}

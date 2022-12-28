Shader "Sturfee/Transparent" {
	Properties{
		_Color("Shadow Color", Color) = (1,1,1,1)
		_ShadowInt("Shadow Intensity", Range(0,1)) = 1.0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}

	SubShader{
		Tags{ 
			"Queue" = "Geometry-1"
			"RenderType" = "TransparentCutout"
			"IgnoreProjector" = "False"
		}
		LOD 200
		Blend Zero SrcColor		
		Offset 0, -1
		ZWrite On

		CGPROGRAM

		#pragma surface surf ShadowOnly alphatest:_Cutoff fullforwardshadows

		fixed4 _Color;
		float _ShadowInt;

		struct Input {
			float2 uv_MainTex;
		};

		inline fixed4 LightingShadowOnly(SurfaceOutput s, fixed3 lightDir, fixed atten) {
			fixed4 c;
			c.rgb = s.Albedo*atten;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = _LightColor0 + _LightColor0 * _Color;
			o.Albedo = lerp(float3(1.0, 1.0, 1.0), c.rgb, _ShadowInt); //c.rgb;
			o.Alpha = 1.0f;
		}
		ENDCG		
	}

	Fallback "Transparent/Cutout/VertexLit"
}
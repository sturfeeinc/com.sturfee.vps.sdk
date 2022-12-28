#ifndef DC_WGRID_CORE_MOBILE_INCLUDED
#define DC_WGRID_CORE_MOBILE_INCLUDED

// Fix for Unity 5.4 upgrade
//UNITY_SHADER_NO_UPGRADE

// Struct

struct appdata_full_t
{
     float4 vertex    : POSITION;  // The vertex position in model space.
     float3 normal    : NORMAL;    // The vertex normal in model space.
     float4 texcoord  : TEXCOORD0; // The first UV coordinate.
     float4 texcoord1 : TEXCOORD1; // The second UV coordinate.
     float4 texcoord2 : TEXCOORD2; // The second UV coordinate.
     float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
     float4 color     : COLOR;     // Per-vertex color
};

struct SurfaceOutput_t
{
     half3 Albedo;
     half3 Base;
     half3 Normal;
     half3 Emission;
     half Specular;
     half Gloss;
     half Alpha;
     float w;
};

// Marcos

#define DC_WIREFRAME_COORDS_MOBILE float2 uv_MainTex;float2 uv2_MainTex2;float3 vertex;float3 worldPos;float mass;
#define DC_WIREFRAME_TRANSFER_COORDS_MOBILE(o) o.worldPos.xyz = mul(_Object2World, v.vertex);o.vertex = v.vertex;\
								if(_GridSpacingScale==1.0f||_GridUseWorldspace==1.0f){ if(_GridSpacingScale==_GridUseWorldspace) {DC_WIREFRAME_CALCULATE_SPACE_WORLD(o.mass) } else {DC_WIREFRAME_CALCULATE_SPACE_NORMAL(o.mass)} }else{DC_WIREFRAME_CALCULATE_SPACE_OBJECT(o.mass)}
#define DC_WIREFRAME_TRANSFER_TEX float2 wireframe_tex = TRANSFORM_TEX(((_WireframeUV==0.0f)?i.uv_MainTex.xy:i.uv2_MainTex2.xy), _WireframeTex)+half2(_WireframeTexAniSpeedX,_WireframeTexAniSpeedY)*_Time;\
								float2 wireframeMask_tex = TRANSFORM_TEX(i.uv_MainTex.xy, _WireframeMaskTex);

#if _WIREFRAME_ALPHA_TEX_ALPHA
	#define DC_APPLY_WIREFRAME_MOBILE(col,alpha,i,w) DC_WIREFRAME_MASS(i.mass,i.worldPos) DC_WIREFRAME_TRANSFER_TEX float w=DC_APPLY_WIREFRAME_COLOR_TEX(col,mass,wireframe_tex);
#elif _WIREFRAME_ALPHA_TEX_ALPHA_INVERT
	#define DC_APPLY_WIREFRAME_MOBILE(col,alpha,i,w) DC_WIREFRAME_MASS(i.mass,i.worldPos) DC_WIREFRAME_TRANSFER_TEX float w=DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(col,mass,1-alpha,wireframe_tex);alpha+=w;
#elif _WIREFRAME_ALPHA_MASK
	#define DC_APPLY_WIREFRAME_MOBILE(col,alpha,i,w) DC_WIREFRAME_MASS(i.mass,i.worldPos) DC_WIREFRAME_TRANSFER_TEX float w=DC_APPLY_WIREFRAME_COLOR_TEX_MASK(col,mass,wireframe_tex,wireframeMask_tex);alpha+=w;
#else	
	#define DC_APPLY_WIREFRAME_MOBILE(col,alpha,i,w) DC_WIREFRAME_MASS(i.mass,i.worldPos) DC_WIREFRAME_TRANSFER_TEX;float w=DC_APPLY_WIREFRAME_COLOR_TEX(col,mass,wireframe_tex);alpha+=w;
#endif

#endif //DC_WIREFRAME_CORE_INCLUDED
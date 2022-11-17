#ifndef DC_WGRID_CORE_INCLUDED
#define DC_WGRID_CORE_INCLUDED

// Fix for Unity 5.4 upgrade
//UNITY_SHADER_NO_UPGRADE
#if UNITY_VERSION >= 540
	#define _Object2World unity_ObjectToWorld
	#define _World2Object unity_WorldToObject
	#define _Projector unity_Projector
	#define _ProjectorClip unity_ProjectorClip

	#define DC_WIREFRAME_PROJECTOR_VAR\
		uniform float4x4 unity_Projector;\
		uniform float4x4 unity_ProjectorClip;
#else
	#define DC_WIREFRAME_PROJECTOR_VAR\
		uniform float4x4 _Projector;\
		uniform float4x4 _ProjectorClip;
#endif

// Properties

uniform fixed4 _WireframeColor;
uniform float _WireframeSize;
uniform float _WireframeUV;
uniform sampler2D _WireframeMaskTex;
uniform float4 _WireframeMaskTex_ST;
uniform sampler2D _WireframeTex;
uniform float4 _WireframeTex_ST;
uniform float _WireframeTexAniSpeedX;
uniform float _WireframeTexAniSpeedY;
uniform float _WireframeSizeScale;
uniform float _GridSpacing;
uniform float _GridSpacingScale;
uniform float _GridUseWorldspace;
uniform float4 _WireframeEmissionColor;
uniform float _WireframeAlphaCutoff;

// Helpers	

inline float DrawWireframeAA(float3 n, float width)
{
	width*=1.9f;
	float a = 1.0f;
	float3 w =fwidth(abs(n.xyz));
	float check1 = _ProjectionParams.y*0.1f;
	float check2 = _ProjectionParams.y*0.001f;
	if(w.y<=check1&&w.y>check2) w.y=check1;	
	if(w.x<=check1&&w.x>check2) w.x=check1;
	if(w.z<=check1&&w.z>check2) w.z=check1;	
//// AA
	#if _WIREFRAME_AA
	half ww = (width-1.0f);
	ww=(ww>1.0f)?ww:0.0f;
	half3 steps = smoothstep(w*ww,w*width,n.rgb*2);
	a = min(min(steps.x, steps.y), steps.z);
// NoAA
	#else
	if(width==0.0f){
		a=0.0f;	
	} else {
//		float3 steps = clamp(w*width*n.rgb*2,0,1);//smoothstep(w*width,w*width,n.rgb*2);
//		steps = steps*steps*(3-2*steps);
	float3 steps = smoothstep(w*width,w*width+0.000000000001f,n.rgb*3.0f);
		a = min(min(steps.x, steps.y), steps.z);
	}
	#endif	 

	return a;
}

inline float DC_APPLY_WIREFRAME_COLOR_TEX(inout fixed3 col,float3 n,float2 uv)
{	
	fixed4 tex = tex2D(_WireframeTex, uv);
	float w = DrawWireframeAA(n,_WireframeSize);
	w = (1.0f-w)*_WireframeColor.a*tex.a;
	#ifndef _EMISSION
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb,w);
	#else
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb+_WireframeEmissionColor.rgb,w);
	#endif
	return w;
}

inline float DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(inout fixed3 col,float3 n,float alpha, float2 uv)
{
	fixed4 tex = tex2D(_WireframeTex, uv);
	float w = DrawWireframeAA(n,_WireframeSize);

	// Calculate alpha cutoff
	float cutoffAlpha = alpha;
	if(alpha<_WireframeAlphaCutoff) cutoffAlpha=0.0f;
	w = (1.0f-w)*_WireframeColor.a*cutoffAlpha*tex.a;
	
	// w = (1-w)*_WireframeColor.a*alpha*tex.a;
	#ifndef _EMISSION
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb,w);
	#else
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb+_WireframeEmissionColor.rgb,w);
	#endif
	return w;
}

inline float DC_APPLY_WIREFRAME_COLOR_TEX_MASK(inout fixed3 col,float3 n, float2 uv, float2 uv2){
	fixed4 tex = tex2D(_WireframeTex, uv);
	fixed mask = tex2D(_WireframeMaskTex, uv2).a;
	float w = DrawWireframeAA(n,_WireframeSize);
	w = (1.0f-w)*mask*_WireframeColor.a*tex.a;
	#ifndef _EMISSION
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb,w);
	#else
	col.rgb = lerp(col.rgb,_WireframeColor.rgb*tex.rgb+_WireframeEmissionColor.rgb,w);
	#endif
	return w;
}


// Marcos

#define DC_WIREFRAME_COORDS(idx1,idx2) float4 worldPos:COLOR;float3 vertex:COLOR1;float4 wireframe_tex:TEXCOORD##idx1;
//float2 wireframe_mask_tex:TEXCOORD##idx2;

#define DC_WIREFRAME_CALCULATE_SPACE_NORMAL(input) input=_GridSpacing;

#define DC_WIREFRAME_CALCULATE_SPACE_OBJECT(input) float scale = length(float3(_World2Object[0].x, _World2Object[1].x, _World2Object[2].x));input=scale*_GridSpacing;

#define DC_WIREFRAME_CALCULATE_SPACE_WORLD(input) float scale = length(float3(_Object2World[0].x, _Object2World[1].x, _Object2World[2].x));input=scale*_GridSpacing;

#define DC_WIREFRAME_MASS(size,pos)\
float width=size*1.9f;\
float3 space = (_GridUseWorldspace==0.0f)?i.vertex.xyz:pos.xyz;\
float3 mass = abs(frac(space/width));\
mass=mass*mass;\
if(mass.x<=0.5f) mass.x=mass.x*2.0f; else mass.x=(1.0f-mass.x)*2.0f;\
if(mass.y<=0.5f) mass.y=mass.y*2.0f; else mass.y=(1.0f-mass.y)*2.0f;\
if(mass.z<=0.5f) mass.z=mass.z*2.0f; else mass.z=(1.0f-mass.z)*2.0f;
// float3 mass = abs(frac(space/width)*frac(space/width));\
// if(mass.x<=0.5f) mass.x=mass.x*2.0f; else mass.x=(1-mass.x)*2.0f;\
// if(mass.y<=0.5f) mass.y=mass.y*2.0f; else mass.y=(1-mass.y)*2.0f;\
// if(mass.z<=0.5f) mass.z=mass.z*2.0f; else mass.z=(1-mass.z)*2.0f;	



#define DC_WIREFRAME_TRANSFER_COORDS(o) o.worldPos.xyz = mul(_Object2World, v.vertex);\
	o.vertex = v.vertex;\
	o.wireframe_tex.xy=TRANSFORM_TEX(((_WireframeUV==0.0f)?v.uv0:v.uv1),_WireframeTex)+float2(_WireframeTexAniSpeedX,_WireframeTexAniSpeedY)*_Time.y;\
	o.wireframe_tex.zw=TRANSFORM_TEX(v.uv0,_WireframeMaskTex);\
	if(_GridSpacingScale==1.0f||_GridUseWorldspace==1.0f){ if(_GridSpacingScale==_GridUseWorldspace) {DC_WIREFRAME_CALCULATE_SPACE_WORLD(o.worldPos.w) } else {DC_WIREFRAME_CALCULATE_SPACE_NORMAL(o.worldPos.w)} }else{DC_WIREFRAME_CALCULATE_SPACE_OBJECT(o.worldPos.w)}

#if _WIREFRAME_ALPHA_TEX_ALPHA
	#define DC_APPLY_WIREFRAME(col,alpha,i) DC_WIREFRAME_MASS(i.worldPos.w,i.worldPos) float w=DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(col,mass,alpha,i.wireframe_tex.xy);//alpha=w;
#elif _WIREFRAME_ALPHA_TEX_ALPHA_INVERT
	#define DC_APPLY_WIREFRAME(col,alpha,i) DC_WIREFRAME_MASS(i.worldPos.w,i.worldPos) float w=DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(col,mass,1-alpha,i.wireframe_tex.xy);alpha+=w;
#elif _WIREFRAME_ALPHA_MASK
	#define DC_APPLY_WIREFRAME(col,alpha,i) DC_WIREFRAME_MASS(i.worldPos.w,i.worldPos) float w=DC_APPLY_WIREFRAME_COLOR_TEX_MASK(col,mass,i.wireframe_tex.xy,i.wireframe_tex.zw);alpha+=w;
#else
	#define DC_APPLY_WIREFRAME(col,alpha,i) DC_WIREFRAME_MASS(i.worldPos.w,i.worldPos) float w=DC_APPLY_WIREFRAME_COLOR_TEX(col,mass,i.wireframe_tex.xy);alpha+=w;
#endif	


// For Standard Shader

#define DC_WIREFRAME_COORDS_STANDARD(idx1,idx2) float3 vertex:COLOR2;float4 wireframe_tex:TEXCOORD##idx1;
//float2 wireframe_mask_tex:TEXCOORD##idx2;

#define DC_WIREFRAME_TRANSFER_COORDS_STANDARD(o) o.posWorld.xyz = mul(_Object2World, v.vertex);\
	o.vertex = v.vertex;\
	o.wireframe_tex.xy=TRANSFORM_TEX(((_WireframeUV==0.0f)?v.uv0:v.uv1),_WireframeTex)+float2(_WireframeTexAniSpeedX,_WireframeTexAniSpeedY)*_Time.y;\
	o.wireframe_tex.zw=TRANSFORM_TEX(v.uv0,_WireframeMaskTex);\
	if(_GridSpacingScale==1.0f||_GridUseWorldspace==1.0f){ if(_GridSpacingScale==_GridUseWorldspace) {DC_WIREFRAME_CALCULATE_SPACE_WORLD(o.posWorld.w) } else {DC_WIREFRAME_CALCULATE_SPACE_NORMAL(o.posWorld.w)} }else{DC_WIREFRAME_CALCULATE_SPACE_OBJECT(o.posWorld.w)}

#if _WIREFRAME_ALPHA_TEX_ALPHA
	#define DC_APPLY_WIREFRAME_STANDARD(col,alpha,i) DC_WIREFRAME_MASS(i.posWorld.w,i.posWorld) float w=DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(col,mass,alpha,i.wireframe_tex.xy);//alpha=w;
#elif _WIREFRAME_ALPHA_TEX_ALPHA_INVERT
	#define DC_APPLY_WIREFRAME_STANDARD(col,alpha,i) DC_WIREFRAME_MASS(i.posWorld.w,i.posWorld) float w=DC_APPLY_WIREFRAME_COLOR_TEX_ALPHA(col,mass,1-alpha,i.wireframe_tex.xy);alpha+=w;
#elif _WIREFRAME_ALPHA_MASK
	#define DC_APPLY_WIREFRAME_STANDARD(col,alpha,i) DC_WIREFRAME_MASS(i.posWorld.w,i.posWorld) float w=DC_APPLY_WIREFRAME_COLOR_TEX_MASK(col,mass,i.wireframe_tex.xy,i.wireframe_tex.zw);alpha+=w;
#else
	#define DC_APPLY_WIREFRAME_STANDARD(col,alpha,i) DC_WIREFRAME_MASS(i.posWorld.w,i.posWorld) float w=DC_APPLY_WIREFRAME_COLOR_TEX(col,mass,i.wireframe_tex.xy);alpha+=w;
#endif	

#endif //DC_WIREFRAME_CORE_INCLUDED
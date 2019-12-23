// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "FX/Water" {
Properties {
	_WaveScale ("Wave scale", Range (0.02,0.15)) = 0.063
	_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
	_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
	_RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
	[NoScaleOffset] _Fresnel ("Fresnel (A) ", 2D) = "gray" {}
	[NoScaleOffset] _BumpMap ("Normalmap ", 2D) = "bump" {}
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	_ReflectiveColor ("Reflective color (RGB) fresnel (A) ", COLOR) = (.5, .5, .5, 1)
	_HorizonColor ("Simple water horizon color", COLOR)  = ( .172, .463, .435, 1)
	_LeftReflectionTex ("Internal Reflection Left", 2D) = "" {}
	_RightReflectionTex ("Internal Reflection Right", 2D) = "" {}
	[HideInInspector] _RefractionTex ("Internal Refraction", 2D) = "" {}
}


// -----------------------------------------------------------
// Fragment program cards


Subshader {
	Tags { "WaterMode"="Refractive" "RenderType"="Opaque" }
	Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE

#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
#define HAS_REFLECTION 1
#endif
#if defined (WATER_REFRACTIVE)
#define HAS_REFRACTION 1
#endif


#include "UnityCG.cginc"

uniform float4 _WaveScale4;
uniform float4 _WaveOffset;

#if HAS_REFLECTION
uniform float _ReflDistort;
#endif
#if HAS_REFRACTION
uniform float _RefrDistort;
#endif

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f {
	float4 pos : SV_POSITION;
	#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
		float4 ref : TEXCOORD0;
		float2 bumpuv0 : TEXCOORD1;
		float2 bumpuv1 : TEXCOORD2;
		float3 viewDir : TEXCOORD3;
	#else
		float2 bumpuv0 : TEXCOORD0;
		float2 bumpuv1 : TEXCOORD1;
		float3 viewDir : TEXCOORD2;
	#endif
	UNITY_FOG_COORDS(4)
};

// Same as standard ComputeScreenPos() except that it doesn't call TransformStereoScreenSpaceTex()
// when stereo instance rendering is enabled. This is important because we need to be able to sample
// from the entire reflection texture, and not just the left/right half, which is what the normal
// ComputeScreenPos() would get us.
inline float4 ComputeScreenPosIgnoreStereo(float4 pos) {
	float4 o = pos * 0.5f;
#if defined(UNITY_HALF_TEXEL_OFFSET)
	o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _ScreenParams.zw;
#else
	o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
#endif
	o.zw = pos.zw;
	return o;
}

v2f vert(appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	

	// scroll bump waves
	float4 temp;
	float4 wpos = mul (unity_ObjectToWorld, v.vertex);
	temp.xyzw = wpos.xzxz * _WaveScale4 + _WaveOffset;
	o.bumpuv0 = temp.xy;
	o.bumpuv1 = temp.wz;
	
	// object space view direction (will normalize per pixel)
	o.viewDir.xzy = WorldSpaceViewDir(v.vertex);
	
	#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
	o.ref = ComputeScreenPosIgnoreStereo(o.pos);
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
sampler2D _LeftReflectionTex;
sampler2D _RightReflectionTex;
#endif
#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
float4 _ReflectiveColor;
#endif
#if defined (WATER_REFRACTIVE)
sampler2D _Fresnel;
sampler2D _RefractionTex;
uniform float4 _RefrColor;
#endif
#if defined (WATER_SIMPLE)
uniform float4 _HorizonColor;
#endif
sampler2D _BumpMap;

half4 frag( v2f i ) : SV_Target
{
	i.viewDir = normalize(i.viewDir);
	
	// combine two scrolling bumpmaps into one
	half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
	half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
	half3 bump = (bump1 + bump2) * 0.5;
	
	// fresnel factor
	half fresnelFac = dot( i.viewDir, bump );
	
	// perturb reflection/refraction UVs by bumpmap, and lookup colors
	
	#if HAS_REFLECTION
	bool leftEye;

	#ifdef UNITY_SINGLE_PASS_STEREO
	leftEye = unity_StereoEyeIndex == 0;
	#else
	leftEye = (unity_CameraProjection[0][2] <= 0);
	#endif

	float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
	half4 refl;

	if (leftEye)
	{
		refl = tex2Dproj(_LeftReflectionTex, UNITY_PROJ_COORD(uv1));
	}
	else
	{
		refl = tex2Dproj(_RightReflectionTex, UNITY_PROJ_COORD(uv1));
	}

	#endif
	#if HAS_REFRACTION
	float4 uv2 = i.ref; uv2.xy -= bump * _RefrDistort;
	half4 refr = tex2Dproj( _RefractionTex, UNITY_PROJ_COORD(uv2) ) * _RefrColor;
	#endif
	
	// final color is between refracted and reflected based on fresnel
	half4 color;
	
	#if defined(WATER_REFRACTIVE)
	half fresnel = UNITY_SAMPLE_1CHANNEL( _Fresnel, float2(fresnelFac,fresnelFac) );
	color = lerp( refr, refl, fresnel );
	#endif
	
	#if defined(WATER_REFLECTIVE)
	half4 water = _ReflectiveColor;
	color.rgb = lerp( water.rgb, refl.rgb, water.a );
	color.a = refl.a * water.a;
	#endif
	
	#if defined(WATER_SIMPLE)
	half4 water = _ReflectiveColor;
	color.rgb = lerp( water.rgb, _HorizonColor.rgb, water.a );
	color.a = _HorizonColor.a;
	#endif

	UNITY_APPLY_FOG(i.fogCoord, color);
	return color;
}
ENDCG

	}
}

}

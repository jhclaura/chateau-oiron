// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Diffuse Doodle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _NoiseScale ("Noise Scale", Float) = 0.1
        _NoiseSnap ("Noise Snap", Float) = 3.0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
        #pragma multi_compile_local _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnitySprites.cginc"

        float _NoiseScale;
        float _NoiseSnap;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color;
        };

        float3 random3(float3 c) {
            float j = 4096.0*sin(dot(c,float3(17.0, 59.4, 15.0)));
            float3 r;
            r.z = frac(512.0*j);
            j *= .125;
            r.x = frac(512.0*j);
            j *= .125;
            r.y = frac(512.0*j);
            return r-0.5;
        }

        inline float snap (float x, float snap)
        {
            return snap * round(x / snap);
        }

        void vert (inout appdata_full v, out Input o)
        {
            v.vertex = UnityFlipSprite(v.vertex, _Flip);

            //float time = snap(_Time.y, _NoiseSnap); //float3(_Time.y, 0, 0);
            //float2 noise = random3(v.vertex.xyz + float3(time, 0.0, 0.0)).xy * _NoiseScale;
            //v.vertex.xy += noise;
            
            v.vertex.x += sin (v.vertex.x * 100.0 + _Time.y)*_NoiseScale;
            v.vertex.y += cos (v.vertex.y * 100.0 +_Time.y)*_NoiseScale;
            

            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color * _RendererColor;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = SampleSpriteTexture (IN.uv_MainTex) * IN.color;
            o.Albedo = c.rgb * c.a;
            o.Alpha = c.a;
        }
        ENDCG
    }

Fallback "Transparent/VertexLit"
}

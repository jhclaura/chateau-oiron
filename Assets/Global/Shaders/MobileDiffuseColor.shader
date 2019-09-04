
// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.
// - support gpu instancing

Shader "Drops/Mobile/Diffuse Color"
{
    Properties {
       _MainTex ("Base (RGB)", 2D) = "white" {}
       _Color("Color", Color) = (0,0,0,0)
	   _TextureST("Texture ST", vector) = (1,1,0,0)
    }
    SubShader {
       Tags { "RenderType" = "Opaque" }
       LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd

        sampler2D _MainTex;
		//float4 _MainTex_ST;

        //float4 _Color;
		UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		   UNITY_DEFINE_INSTANCED_PROP(fixed4, _TextureST)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input {
           float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
			float4 texST = UNITY_ACCESS_INSTANCED_PROP(Props, _TextureST);
			float2 newUV = frac(IN.uv_MainTex * texST.xy + texST.zw);
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			fixed4 c = tex2D(_MainTex, newUV) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
        }
        ENDCG
    }

    Fallback "Drops/Mobile/VertexLitInstancing"
}
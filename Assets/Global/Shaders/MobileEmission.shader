
Shader "Drops/Mobile/Emission Color"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_EmissionColor("Emission Color", Color) = (0,0,0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 150

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd
		#pragma shader_feature _EMISSION

		//================================================================================================================================
		// STRUCTS
		sampler2D _MainTex;
        //float4 _Color;
		half4 _EmissionColor;

		struct Input
		{
			float2 uv_MainTex;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

		//================================================================================================================================
		// SURFACE FUNCTION

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 mainTex = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			o.Albedo = mainTex.rgb;
			o.Alpha = mainTex.a;
			o.Emission += _EmissionColor.rgb;
		}
		ENDCG
	}

	FallBack "Drops/Mobile/VertexLitInstancing"
}


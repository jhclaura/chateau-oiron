// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:33522,y:32472,varname:node_4013,prsc:2|emission-1304-RGB,alpha-6758-OUT;n:type:ShaderForge.SFN_Color,id:1304,x:33036,y:32324,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_1304,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_NormalVector,id:1019,x:31956,y:33722,prsc:2,pt:False;n:type:ShaderForge.SFN_Slider,id:8224,x:31582,y:33419,ptovrint:False,ptlb:node_8224,ptin:_node_8224,varname:node_8224,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:7040,x:31569,y:33590,ptovrint:False,ptlb:node_7040,ptin:_node_7040,varname:node_7040,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Lerp,id:3937,x:32147,y:33498,varname:node_3937,prsc:2|A-8224-OUT,B-7040-OUT,T-7342-OUT;n:type:ShaderForge.SFN_Vector1,id:7342,x:31811,y:33698,varname:node_7342,prsc:2,v1:0;n:type:ShaderForge.SFN_ComponentMask,id:9649,x:32116,y:33615,varname:node_9649,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-1019-OUT;n:type:ShaderForge.SFN_Multiply,id:2756,x:32358,y:33544,varname:node_2756,prsc:2|A-9649-OUT,B-3937-OUT;n:type:ShaderForge.SFN_Slider,id:5912,x:32460,y:32684,ptovrint:False,ptlb:Fresnel_Strength,ptin:_Fresnel_Strength,varname:node_5912,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:5331,x:32858,y:32524,varname:node_5331,prsc:2|A-6227-OUT,B-5912-OUT;n:type:ShaderForge.SFN_OneMinus,id:6227,x:32690,y:32459,varname:node_6227,prsc:2|IN-3306-OUT;n:type:ShaderForge.SFN_Fresnel,id:3306,x:32520,y:32459,varname:node_3306,prsc:2|EXP-4857-OUT;n:type:ShaderForge.SFN_Slider,id:4857,x:32161,y:32289,ptovrint:False,ptlb:Fresnel_Exp,ptin:_Fresnel_Exp,varname:node_4857,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6353885,max:1;n:type:ShaderForge.SFN_Multiply,id:6758,x:33263,y:32606,varname:node_6758,prsc:2|A-1304-A,B-5331-OUT;proporder:1304-5912-4857;pass:END;sub:END;*/

Shader "Shader Forge/soft edge" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Fresnel_Strength ("Fresnel_Strength", Range(0, 1)) = 1
        _Fresnel_Exp ("Fresnel_Exp", Range(0, 1)) = 0.6353885
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Fresnel_Strength;
            uniform float _Fresnel_Exp;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float3 emissive = _Color.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,(_Color.a*((1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel_Exp))*_Fresnel_Strength)));
            }
            ENDCG
        }
        /*
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        */
    }
    FallBack "Diffuse"
    //CustomEditor "ShaderForgeMaterialInspector"
}

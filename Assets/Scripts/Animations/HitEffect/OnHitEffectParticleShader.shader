Shader "Custom/Particles/OnHitEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Fade ("Fade", Float) = 1.0
        [NoScaleOffset]_MainTex ("MainTex", 2D) = "white" {}
    }

    SubShader
    {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "QuadSpriteLike"
            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Fade;
                float4 _MainTex_ST;
            CBUFFER_END

            #if defined(DOTS_INSTANCING_ON)
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _Color)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Fade)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MainTex_ST)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
            #define _Fade UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Fade)
            #define _MainTex_ST UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _MainTex_ST)
            #endif

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // <- particle color support
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.positionCS = TransformObjectToHClip(v.vertex);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 texCol = tex2D(_MainTex, i.uv);

                float2 centerUV = i.uv - 0.5;
                float dist = length(centerUV);
                float edgeFalloff = smoothstep(0.5, 0.25, dist);

                texCol *= _Color;
                texCol *= i.color; // apply vertex color (particle tint)
                texCol.a *= _Fade;
                texCol.a *= edgeFalloff;
                texCol.rgb *= 1.5;

                return texCol;
            }

            ENDHLSL
        }
    }
}

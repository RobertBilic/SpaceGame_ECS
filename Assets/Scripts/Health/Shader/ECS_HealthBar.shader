Shader "Custom/ECS_HealthBar"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("MainTex", 2D) = "white" {}
        _Color ("Fill Color", Color) = (0.2, 1, 0.2, 1)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 0.6)
        _Fill ("Fill", Range(0, 1)) = 1
        _EdgeFade ("Edge Fade", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "QuadHealthBarTextured"
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
                float4 _BackgroundColor;
                float _Fill;
                float _EdgeFade;
            CBUFFER_END

            #if defined(DOTS_INSTANCING_ON)
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _Color)
                UNITY_DOTS_INSTANCED_PROP(float4, _BackgroundColor)
                UNITY_DOTS_INSTANCED_PROP(float, _Fill)
                UNITY_DOTS_INSTANCED_PROP(float, _EdgeFade)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
            #define _BackgroundColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BackgroundColor)
            #define _Fill UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Fill)
            #define _EdgeFade UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _EdgeFade)
            #endif

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.positionCS = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    float2 uv = i.uv;

    // Horizontal fill fade
    float fillEdge = 1.0 - smoothstep(_Fill - _EdgeFade, _Fill + _EdgeFade, uv.x);

    // Only horizontal fade — no vertical fade
    float alpha = lerp(_BackgroundColor.a, _Color.a, fillEdge);

    float3 blendedColor = lerp(_BackgroundColor.rgb, _Color.rgb, fillEdge);
    float4 texColor = tex2D(_MainTex, uv);
    texColor.rgb *= blendedColor;
    texColor.a *= alpha;

    return texColor;
}
            ENDHLSL
        }
    }
}

Shader "Custom/ECS_QuadSpriteLike"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (1,1,1,1)
        _DrawOrder("Draw Order", Int) = 1
    }

    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

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

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float _DrawOrder;
            CBUFFER_END

            #if defined(DOTS_INSTANCING_ON)
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP(float4, _Color)
                UNITY_DOTS_INSTANCED_PROP(float4, _MainTex_ST)
                UNITY_DOTS_INSTANCED_PROP(float, _DrawOrder)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
            #define _MainTex_ST UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _MainTex_ST)
            #define _DrawOrder UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DrawOrder)
            #endif

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
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
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 vertexPos = v.vertex.xyz;

                // Offset Z based on _DrawOrder (positive = further back)
                vertexPos.z -= _DrawOrder * 0.1;

                o.positionCS = TransformObjectToHClip(vertexPos);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return texCol * _Color;
            }

            ENDHLSL
        }
    }
}

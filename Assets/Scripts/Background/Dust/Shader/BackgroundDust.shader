Shader "Custom/BackgroundDust_ECS"
{
    Properties
    {
        _DustDensity ("Dust Density", Float) = 0.3
        _DustTileDensity ("Dust Tile Density", Float) = 50
        _DustSizeMin ("Dust Size Min", Float) = 0.02
        _DustSizeMax ("Dust Size Max", Float) = 0.08
        _ScrollSpeed ("Scroll Speed", Vector) = (0.01, 0.002, 0, 0)
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _DustColorMin ("Dust Color Min", Color) = (0.7,0.7,0.7,1)
        _DustColorMax ("Dust Color Max", Color) = (1,1,1,1)
        _UVScale ("UV Scale", Vector) = (20, 20, 0, 0)
        _RandomSeed ("UV Scale", Float) = 0
    }

    SubShader
    {
        Tags { 
             "RenderType"="Transparent" 
             "Queue"="Transparent" 
            "DisableBatching"="True" 
            "IgnoreProjector"="True"
            "RenderLayer" = "Default"
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint   instanceID  : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                uint   instanceID  : SV_InstanceID;
            };

            // SRP Batcher compatible properties
            CBUFFER_START(UnityPerMaterial)
                float _DustDensity;
                float _DustTileDensity;
                float _DustSizeMin;
                float _DustSizeMax;
                float4 _ScrollSpeed;
                float4 _BackgroundColor;
                float4 _DustColorMin;
                float4 _DustColorMax;
                float4 _UVScale;
                float _RandomSeed;
            CBUFFER_END
            
        #if defined(DOTS_INSTANCING_ON)
            // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _DustDensity)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _DustTileDensity)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _DustSizeMin)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _DustSizeMax)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float2, _ScrollSpeed)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float2, _UVScale)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _BackgroundColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _DustColorMin)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _DustColorMax)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _RandomSeed)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float, _DustDensity)
            UNITY_DEFINE_INSTANCED_PROP(float, _DustTileDensity)
            UNITY_DEFINE_INSTANCED_PROP(float, _DustSizeMin)
            UNITY_DEFINE_INSTANCED_PROP(float, _DustSizeMax)
            UNITY_DEFINE_INSTANCED_PROP(float2, _ScrollSpeed)
            UNITY_DEFINE_INSTANCED_PROP(float2, _UVScale)
            UNITY_DEFINE_INSTANCED_PROP(float4, _BackgroundColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _DustColorMin)
            UNITY_DEFINE_INSTANCED_PROP(float4, _DustColorMax)
            UNITY_DEFINE_INSTANCED_PROP(float, _RandomSeed)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif


            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float2 randomOffset = float2(
                    frac(sin(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_RandomSeed,float) * 12.9898) * 43758.5453),
                    frac(sin(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_RandomSeed,float) * 78.233) * 43758.5453)
                );

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv * UNITY_ACCESS_HYBRID_INSTANCED_PROP(_UVScale, float2).xy + UNITY_ACCESS_HYBRID_INSTANCED_PROP(_ScrollSpeed, float2).xy * _Time.y;

                
				#if UNITY_ANY_INSTANCING_ENABLED
				    o.instanceID = v.instanceID;
				#endif

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float2 uv = i.uv;
                float2 cellUV = uv * UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustTileDensity, float);
                float2 cell = floor(cellUV);
                float2 localUV = frac(cellUV);

                half4 col = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_BackgroundColor,float4);
                float randomSeed = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_RandomSeed, float);

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighborCell = cell + float2(x, y);

                        float randDensity = hash(neighborCell + randomSeed);
                        if (randDensity > UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustDensity, float))
                            continue;

                        float2 dustOffset = float2(hash(neighborCell + randomSeed + 2.7), hash(neighborCell.yx + randomSeed + 5.3));
                        float2 dustPos = dustOffset;

                        float dustRadius = lerp(
                            UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustSizeMin,float),
                            UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustSizeMax,float),
                            hash(neighborCell + randomSeed + 7.1)
                        );

                        float2 neighborUV = localUV - float2(x, y);
                        float dist = distance(neighborUV, dustPos);

                        if (dist < dustRadius)
                        {
                            float colorRand = hash(neighborCell + randomSeed + 13.1);
                            float flicker = 0.85 + 0.15 * sin(_Time.y * 2.0 + colorRand * 10.0);

                            half4 dustColor = lerp(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustColorMin,float4), UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DustColorMax,float4), colorRand);

                            col = dustColor * flicker;
                        }
                    }
                }

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
Shader "Custom/SlotHighlightCircle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _PulseSpeed ("Pulse Speed", Float) = 2.0
    }
    SubShader
    {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SlotHighlight"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _PulseSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 centerUV = i.uv - 0.5;
                float dist = length(centerUV);
                float pulse = sin(_Time.y * _PulseSpeed) * 0.25 + 0.75;
                float alpha = smoothstep(0.5, 0.4, dist) * pulse;
                half4 tex = tex2D(_MainTex, i.uv);
                return tex * _Color * alpha;
            }
            ENDHLSL
        }
    }
}

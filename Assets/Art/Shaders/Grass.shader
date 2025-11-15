Shader "Custom/Grass"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _MaskMap("Mask Map", 2D) = "white" {}

        _NormalScale("Normal Scale", Range(0,5)) = 1
        _Smoothness("Smoothness", Range(0,1)) = 0.3
        _AO("Ambient Occlusion", Range(0,4)) = 1
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.3

        _WindStrength("Wind Strength", Range(0,1)) = 0.2
        _WindSpeed("Wind Speed", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.5
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:Setup
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float3> instancePositions: register(t0);
            StructuredBuffer<float4> instanceRotations : register(t1);
            StructuredBuffer<float> instanceScales : register(t2);

            int _VertexCountPerInstance;

            TEXTURE2D(_BaseMap);          SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);        SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);          SAMPLER(sampler_MaskMap);

            float _NormalScale;
            float _Smoothness;
            float _AO;
            float _Cutoff;

            float _WindStrength;
            float _WindSpeed;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                uint vertexID     : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            float3 RotateY(float3 pos, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3(pos.x * c - pos.z * s, pos.y, pos.x * s + pos.z * c);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                uint instanceID = IN.vertexID / _VertexCountPerInstance;

                float3 worldPos = instancePositions[instanceID];
                float3 worldRot = instanceRotations[instanceID];
                float scale     = instanceScales[instanceID];

                float t = _Time.y * _WindSpeed;
                float sway = sin(t + worldPos.x * 0.1 + worldPos.z * 0.1) * _WindStrength;

                float3 pos = IN.positionOS;
                pos.xz += sway;

                pos = RotateY(pos, radians(worldRot.y));
                pos *= scale;

                pos += worldPos;

                OUT.positionHCS = TransformWorldToHClip(pos);
                OUT.uv = IN.uv;

                float3 normal = IN.normalOS;
                normal = RotateY(normal, radians(worldRot.y));
                OUT.normalWS = TransformObjectToWorldNormal(normal);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                if (baseCol.a < _Cutoff)
                    discard;

                return baseCol;
            }

            ENDHLSL
        }
    }
}

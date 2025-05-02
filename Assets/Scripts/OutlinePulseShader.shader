Shader "Custom/OutlinePulseShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.05
        _Pulse ("Pulse Intensity", Range(0,1)) = 0
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        // Base Pass: Render the object with base color
        Pass
        {
            Name "Base"
            Tags { "LightMode"="UniversalForward" } // URP forward rendering

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }

        // Outline Pass: Render the outline with pulsing emission
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="UniversalForward" }
            Cull Front // Cull front faces to show outline

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _Pulse;
                float4 _EmissionColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Fixed outline width
                float3 offset = IN.normalOS * _OutlineWidth;
                float4 positionOS = IN.positionOS + float4(offset, 0);
                OUT.positionCS = TransformObjectToHClip(positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Pulse emission color: lerp between dark and bright emission
                float3 darkEmission = float3(0.1, 0.1, 0.1); // Dark emission
                float3 brightEmission = float3(1.0, 1.0, 1.0); // Bright emission
                float3 emission = lerp(darkEmission, brightEmission, _Pulse);

                // Apply emission color
                float3 finalEmission = _EmissionColor.rgb * emission;

                // Return the outline color with emission
                return float4(_OutlineColor.rgb + finalEmission, _OutlineColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
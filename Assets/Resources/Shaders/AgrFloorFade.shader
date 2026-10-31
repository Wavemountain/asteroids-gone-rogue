// Arena floor. Standard in the middle, radial fade into the void, clipped at the dome.
// Shader.Find("AGR/FloorFade").
Shader "AGR/FloorFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.04
        _Void ("Void", Color) = (0.015, 0.022, 0.045, 1)
        _FadeStart ("Fade Start", Float) = 24
        _FadeEnd ("Fade End", Float) = 30
        _FadeLead ("Fade Lead", Float) = 3
        _FadeSpan ("Fade Span", Float) = 4
        _DistTex ("Contour", 2D) = "gray" {}
        _DistReach ("Contour Reach", Float) = 50
        _ClipReach ("Clip Reach", Float) = 50
        _UseContour ("Use Contour", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        sampler2D _DistTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        fixed4 _Void;
        float _FadeStart;
        float _FadeEnd;
        float _FadeLead;
        float _FadeSpan;
        float _DistReach;
        float _ClipReach;
        float _UseContour;
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float dist = length(IN.worldPos.xz);
            float t;
            float clipDist;
            if (_UseContour > 0.5 && _DistReach > 1.0)
            {
                float2 uv = IN.worldPos.xz / (2.0 * _DistReach) + 0.5;
                float encoded = tex2D(_DistTex, uv).r;
                float signedDistance = (encoded - 0.5) * 2.0 * _DistReach;
                float fadeSpan = _FadeSpan;
                if (fadeSpan < 0.001)
                {
                    fadeSpan = 4.0;
                }

                t = saturate((-_FadeLead - signedDistance) / fadeSpan);
                float reach = _ClipReach;
                if (reach < 1.0)
                {
                    reach = _DistReach;
                }

                clipDist = reach - dist;
            }
            else
            {
                float span = _FadeEnd - _FadeStart;
                if (span < 0.001)
                {
                    span = 0.001;
                }

                t = saturate((dist - _FadeStart) / span);
                clipDist = (_FadeEnd + 0.02) - dist;
            }

            fixed3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            o.Albedo = lerp(albedo, _Void.rgb, t);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
            clip(clipDist);
        }
        ENDCG
    }
    FallBack "Standard"
}

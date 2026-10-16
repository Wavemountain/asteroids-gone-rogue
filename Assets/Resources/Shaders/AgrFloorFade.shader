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
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        fixed4 _Void;
        float _FadeStart;
        float _FadeEnd;
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float dist = length(IN.worldPos.xz);
            float span = _FadeEnd - _FadeStart;
            if (span < 0.001)
            {
                span = 0.001;
            }

            float t = saturate((dist - _FadeStart) / span);
            fixed3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            o.Albedo = lerp(albedo, _Void.rgb, t);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
            clip((_FadeEnd + 0.02) - dist);
        }
        ENDCG
    }
    FallBack "Standard"
}

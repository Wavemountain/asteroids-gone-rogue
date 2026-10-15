// 0.48 AtmosBot: Standard-like surface shader with Fresnel rim in emission.
// Built-in RP only (project has no URP). Loaded via Shader.Find("AGR/RimLit");
// lives under Resources so it is included in builds. Fallback "Standard".
Shader "AGR/RimLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.35
        _Metallic ("Metallic", Range(0,1)) = 0.2
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,1)
        [HDR] _RimColor ("Rim Color", Color) = (0.416,0.659,0.784,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3.0
        _RimStrength ("Rim Strength", Range(0,4)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half4 _EmissionColor;
        half4 _RimColor;
        half _RimPower;
        half _RimStrength;
        struct Input { float2 uv_MainTex; float3 viewDir; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            half rim = 1.0h - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = _EmissionColor.rgb + _RimColor.rgb * (pow(rim, _RimPower) * _RimStrength);
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}

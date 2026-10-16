// Faint procedural nebula and stars behind the dome.
// Colour is fixed. _Motion only scales the drift, so Reduce does not recolour it.
// No RunRng. Shader.Find("AGR/VoidField").
Shader "AGR/VoidField"
{
    Properties
    {
        _Motion ("Motion", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float _Motion;
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            float Hash13(float3 cell)
            {
                return frac(sin(dot(cell, float3(127.1, 311.7, 74.7))) * 43758.5453);
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.world);
                float motion = _Motion;
                if (motion < 0.0)
                {
                    motion = 0.0;
                }

                float drift = _Time.y * motion;
                float band = sin(dir.x * 2.4 + dir.y * 1.7 + drift * 0.07);
                float band2 = sin(dir.z * 3.1 - dir.x * 1.1 - drift * 0.05);
                float nebula = 0.09 + (0.02 * band) + (0.012 * band2);
                if (nebula < 0.07)
                {
                    nebula = 0.07;
                }

                if (nebula > 0.12)
                {
                    nebula = 0.12;
                }

                float3 col = float3(0.45, 0.55, 0.85) * nebula;
                float3 cell = floor(dir * 56.0 + float3(drift * 0.12, drift * 0.04, 0.0));
                float star = step(0.992, Hash13(cell));
                col += star * float3(0.16, 0.17, 0.20);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}

// Dome lip. Depth-tested so enemies and pillars draw in front of the ring.
// Shader.Find("AGR/DomeRing").
Shader "AGR/DomeRing"
{
    Properties
    {
        _Color ("Color", Color) = (0.416, 0.659, 0.784, 0.30)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 col : COLOR;
            };
            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = v.color * _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}

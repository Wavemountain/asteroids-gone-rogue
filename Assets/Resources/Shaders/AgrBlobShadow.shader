// Soft contact disc. Alpha is the material colour; Reduce does not recolour it.
// Shader.Find("AGR/BlobShadow").
Shader "AGR/BlobShadow"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0.35)
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
                float2 uv : TEXCOORD0;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv * 2.0 - 1.0;
                float dist = length(centered);
                float soft = saturate(1.0 - dist);
                soft = soft * soft;
                return fixed4(0, 0, 0, _Color.a * soft);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}

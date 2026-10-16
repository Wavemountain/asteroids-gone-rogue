// Soft contact shadow. RGB is black, so a normal alpha blend only darkens
// the floor (result luma is never above the floor). Alpha falls smoothly
// from the centre to 0 at the quad edge. There is no rim and no ring.
// Reduce does not recolour it. Shader.Find("AGR/BlobShadow").
// No polygon offset. ContactBlob sits on FloorSurface.PlaceY, 0.02–0.05 m
// above the plate (Lift 0.035). At the play camera (near 0.1, far 280) that
// gap is many depth steps, so Offset -1,-1 is not required to win a z-fight
// with the floor and would only bias the quad toward the camera.
Shader "AGR/BlobShadow"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0.9)
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
                float edge = saturate(dist);
                float band = smoothstep(0.55, 1.0, edge);
                float alpha = _Color.a * (1.0 - band);
                return fixed4(0.0, 0.0, 0.0, alpha);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}

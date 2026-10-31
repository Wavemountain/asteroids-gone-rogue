// Hangar hint glyphs. The dynamic font atlas is Smooth: coverage often lives
// in RGB with alpha stuck near 1, so a white vertex colour still draws as
// gray (~142) and the layer gate scores a soft rise. Pixels above the cutoff
// are opaque white; the rest stay clear. One-pixel stems score near 80.
// Shader.Find("AGR/HintInk").
Shader "AGR/HintInk"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Cutoff ("Cutoff", Range(0, 1)) = 0.40
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        // Screen glyphs composite after the arena. LEqual fails against the
        // capture HDR depth, so the hangar crop kept the soft plate behind them.
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            sampler2D _MainTex;
            fixed4 _Color;
            float _Cutoff;
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.texcoord);
                float rgb = max(tex.r, max(tex.g, tex.b));
                float cover = tex.a;
                // Atlas alpha stuck at 1: the glyph is the gray RGB.
                if (tex.a > 0.98 && rgb < tex.a)
                {
                    cover = rgb;
                }

                float on = cover >= _Cutoff ? 1.0 : 0.0;
                return fixed4(i.color.rgb, i.color.a * on);
            }
            ENDCG
        }
    }
}

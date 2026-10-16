// W3 grid. Unlit transparent, with the beat held at rest under the ship.
// Shader.Find("AGR/Grid"). Resources so builds keep it.
Shader "AGR/Grid"
{
    Properties
    {
        _MainTex ("Grid", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,0.12)
        _Pulse ("Pulse", Float) = 1
        _QuietRadius ("Quiet Radius", Float) = 0
        _ShipX ("Ship X", Float) = 0
        _ShipZ ("Ship Z", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Pulse;
            float _QuietRadius;
            float _ShipX;
            float _ShipZ;
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 world : TEXCOORD1;
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float pulse = _Pulse;
                if (_QuietRadius > 0.001)
                {
                    float dx = i.world.x - _ShipX;
                    float dz = i.world.z - _ShipZ;
                    float dist = sqrt((dx * dx) + (dz * dz));
                    if (dist < _QuietRadius)
                    {
                        pulse = 1.0;
                    }
                }
                float alpha = tex.a * _Color.a * pulse;
                return fixed4(tex.rgb * _Color.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}

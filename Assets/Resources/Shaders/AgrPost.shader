// 0.48 built-in post. Shader.Find("AGR/Post"). Resources so builds keep it.
// Linear project: this pass stays linear. Unity encodes sRGB on present.
// Grade and Neutral tonemap always run in the uber pass together.
// Pass 0 prefilter, pass 1 down, pass 2 up, pass 3 uber.
Shader "AGR/Post"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _Threshold ("Threshold", Float) = 1.10
        _SoftKnee ("Soft Knee", Float) = 0.35
        _ClampMax ("Clamp", Float) = 8
        _Intensity ("Bloom Intensity", Float) = 0.60
        _Scatter ("Scatter", Float) = 0.65
        _Exposure ("Exposure EV", Float) = 0
        _Saturation ("Saturation", Float) = 0
        _Contrast ("Contrast", Float) = 0
        _Vignette ("Vignette", Float) = 0.28
        _VignetteSmooth ("Vignette Smooth", Float) = 0.40
        _Grain ("Grain", Float) = 0.12
        _GrainLum ("Grain Lum", Float) = 0.85
        _GrainTime ("Grain Time", Float) = 0
        _Aspect ("Aspect", Float) = 1.6
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "Prefilter"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Threshold;
            float _SoftKnee;
            float _ClampMax;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                half3 color = tex2D(_MainTex, i.uv).rgb;
                half y = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                half cap = max(_ClampMax, 0.01h);
                half luma = max(y, 1e-4h);
                color *= min(y, cap) / luma;
                y = min(y, cap);
                half knee = max(_Threshold * _SoftKnee, 1e-4h);
                half soft = clamp(y - _Threshold + knee, 0.0h, 2.0h * knee);
                soft = (soft * soft) / (4.0h * knee);
                half contrib = max(soft, y - _Threshold);
                contrib = max(contrib, 0.0h);
                color *= contrib / max(y, 1e-4h);
                color = min(color, cap);
                return half4(color, 1);
            }
            ENDCG
        }
        Pass
        {
            Name "Down"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy;
                half3 a = tex2D(_MainTex, i.uv + texel * float2(-1, -1)).rgb;
                half3 b = tex2D(_MainTex, i.uv + texel * float2(1, -1)).rgb;
                half3 c = tex2D(_MainTex, i.uv + texel * float2(-1, 1)).rgb;
                half3 d = tex2D(_MainTex, i.uv + texel * float2(1, 1)).rgb;
                return half4((a + b + c + d) * 0.25h, 1);
            }
            ENDCG
        }
        Pass
        {
            Name "Up"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BloomTex;
            float4 _MainTex_TexelSize;
            float _Scatter;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy;
                half3 low = tex2D(_MainTex, i.uv).rgb * 4.0h;
                low += tex2D(_MainTex, i.uv + texel * float2(-1, -1)).rgb;
                low += tex2D(_MainTex, i.uv + texel * float2(0, -1)).rgb * 2.0h;
                low += tex2D(_MainTex, i.uv + texel * float2(1, -1)).rgb;
                low += tex2D(_MainTex, i.uv + texel * float2(-1, 0)).rgb * 2.0h;
                low += tex2D(_MainTex, i.uv + texel * float2(1, 0)).rgb * 2.0h;
                low += tex2D(_MainTex, i.uv + texel * float2(-1, 1)).rgb;
                low += tex2D(_MainTex, i.uv + texel * float2(0, 1)).rgb * 2.0h;
                low += tex2D(_MainTex, i.uv + texel * float2(1, 1)).rgb;
                low /= 16.0h;
                half3 high = tex2D(_BloomTex, i.uv).rgb;
                return half4(lerp(low, high, saturate(_Scatter)), 1);
            }
            ENDCG
        }
        Pass
        {
            Name "Uber"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BloomTex;
            float _Intensity;
            float _Exposure;
            float _Saturation;
            float _Contrast;
            float4 _Balance;
            float4 _Lift;
            float4 _Gain;
            float _Vignette;
            float _VignetteSmooth;
            float4 _VignetteColor;
            float _Grain;
            float _GrainLum;
            float _GrainTime;
            float _Aspect;
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half NeutralChannel(half v)
            {
                half a = 0.2h;
                half b = 0.29h;
                half c = 0.24h;
                half d = 0.272h;
                half e = 0.02h;
                half f = 0.3h;
                half white = 5.3h;
                half num = (v * (a * v + c * b) + d * e);
                half den = (v * (a * v + b) + d * f);
                half curve = (num / max(den, 1e-4h)) - (e / f);
                half wnum = (white * (a * white + c * b) + d * e);
                half wden = (white * (a * white + b) + d * f);
                half base = (wnum / max(wden, 1e-4h)) - (e / f);
                return curve / max(base, 1e-4h);
            }
            half4 frag(v2f i) : SV_Target
            {
                half3 color = tex2D(_MainTex, i.uv).rgb;
                half3 bloom = tex2D(_BloomTex, i.uv).rgb;
                color += bloom * _Intensity;
                color *= exp2(_Exposure);
                color *= _Balance.rgb;
                half y = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                color = lerp(y.xxx, color, 1.0h + (_Saturation / 100.0h));
                half3 safeC = max(color, 1e-5h);
                half3 logged = log2(safeC / 0.18h) * (1.0h + (_Contrast / 100.0h));
                color = 0.18h * exp2(logged);
                color = half3(NeutralChannel(color.r), NeutralChannel(color.g), NeutralChannel(color.b));
                color = color * _Gain.rgb + _Lift.rgb * (1.0h - color);
                float2 offset = (i.uv - 0.5) * float2(max(_Aspect, 0.01), 1);
                float dist = length(offset) / 0.85;
                float edge = 1.0 - _VignetteSmooth;
                float mask = 1.0 - _Vignette * smoothstep(edge, 1.0, dist);
                color = lerp(_VignetteColor.rgb, color, saturate(mask));
                float2 pixel = i.uv * _ScreenParams.xy;
                float n = frac(sin(dot(pixel + _GrainTime, float2(12.9898, 78.233))) * 43758.5453);
                n = n * 2.0 - 1.0;
                half ly = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                color += n * _Grain * 0.06h * (1.0h - _GrainLum * saturate(ly));
                return half4(max(color, 0.0h), 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

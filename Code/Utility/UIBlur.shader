Shader "UnityTK/UIBlur"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        _Iterations("Iterations", Int) = 1
        _Radius ("Radius", Float) = 4
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            GrabPass { }
            Pass
            {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ PIXELSNAP_ON
                #pragma multi_compile _ UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                #pragma multi_compile _ UNITY_UV_STARTS_AT_TOP
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float4 screenPosition : TEXCOORD0;
                };

                fixed4 _Color;

                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(IN.vertex);
                    OUT.color = IN.color * _Color;
                    #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap(OUT.vertex);
                    #endif
                    OUT.screenPosition = ComputeScreenPos(OUT.vertex);

                    return OUT;
                }

                sampler2D _GrabTexture;
                sampler2D _AlphaTex;
                float4 _GrabTexture_TexelSize;
                float _AlphaSplitEnabled;
                float _Radius;
                int _Iterations;

                fixed4 frag(v2f IN) : SV_Target
                {
                    IN.screenPosition.xy /= IN.screenPosition.w;
                    if (_ProjectionParams.x < 0)
                        IN.screenPosition.y = 1-IN.screenPosition.y;
                    #if UNITY_UV_STARTS_AT_TOP
                    IN.screenPosition.y = 1-IN.screenPosition.y;
                    #endif

                    float offsetPerIteration = _Radius / (float)_Iterations;
                    float2 uv = IN.screenPosition.xy;
                    float4 c = tex2D(_GrabTexture, uv);
                    int i = 0;
                    for (int x = -_Iterations; x < _Iterations; x++)
                    {
                        for (int y = -_Iterations; y < _Iterations; y++)
                        {
                            c += tex2D(_GrabTexture, uv + (_GrabTexture_TexelSize.xy * (float2(x, y) * offsetPerIteration)));
                            i++;
                        }
                    }

                    c /= 1 + (float)i;

                    #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                    if (_AlphaSplitEnabled)
                        c.a = tex2D(_AlphaTex, uv).r;
                    #endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

                    c.rgb = lerp(c.rgb, IN.color.rgb, IN.color.a);
                    c.a = 1;
                    return c;
                }
            ENDCG
            }
        }
}
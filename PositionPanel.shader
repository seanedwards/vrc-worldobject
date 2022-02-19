// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/PositionPanel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "/Assets/AudioLink/Shaders/AudioLink.cginc"
            #include "/Assets/AudioLink/Shaders/SmoothPixelFont.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };  

            sampler2D _MainTex;
            float4 _MainTex_ST; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const uint numDigits = 5;
                const uint rows = 3;
                const uint cols = numDigits+1;
                float3 worldPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
                
                float2 iuv = i.uv;
                float2 pos = iuv*float2(cols,rows);
                uint2 dig = (uint2)(pos);
                float2 fmxy = float2( 4, 6 ) - (glsl_mod(pos,1.)*float2(4.,6.));
                float2 softness = 2./pow( length( float2( ddx( pos.x ), ddy( pos.y ) ) ), 0.5 );
                
                float value = 0;

                switch(dig.y) {
                    case 0:
                        value = worldPos.x;
                        break;
                    case 1:
                        value = worldPos.y;
                        break;
                    case 2:
                        value = worldPos.z;
                        break;
                }

                if (dig.x == 0) {
                    if (value < 0) {
                        return PrintChar('-', fmxy, softness, 0);
                    } else {
                        return 0;
                    }
                    
                } else {
                    return PrintNumberOnLine(
                    value, // value
                    fmxy, // charUV
                    softness, // softness
                    dig.x, // digit
                    numDigits+1, // digitOffset
                    0, // numFractDigits
                    true, // leadZero
                    0 // offset
                    );

                }
                
            }
            ENDCG
        }
    }
}

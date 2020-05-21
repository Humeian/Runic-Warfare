Shader "Hidden/ScreenShakeVR"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
        

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityWorldToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
            half4 _MainTex_ST;
			float _ShakeFac;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 shookUV = i.uv;
				float shookFac = length(shookUV - 0.5)*2;
				shookUV = lerp(shookUV,float2(0.5,0.5),shookFac * _ShakeFac);
                #if UNITY_SINGLE_PASS_STEREO
                    // If Single-Pass Stereo mode is active, transform the
                    // coordinates to get the correct output UV for the current eye.
                    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
                    shookUV = (shookUV - scaleOffset.zw) / scaleOffset.xy;
                #endif
				
				fixed4 col = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(shookUV, _MainTex_ST));
				return col;
			}
			ENDCG
		}
	}
}

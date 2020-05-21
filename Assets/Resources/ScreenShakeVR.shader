Shader "Hidden/Custom/ScreenShakeVR"
{
HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float _ShakeFac;
    half4 _MainTex_ST;

    float4 frag(VaryingsDefault i) : SV_Target
    {
        float2 shookUV = i.texcoord;
        float shookFac = length(shookUV - 0.5) * 2;
        shookUV = lerp(shookUV, float2(0.5,0.5), shookFac * _ShakeFac);
        #if UNITY_SINGLE_PASS_STEREO
            UnityStereoTransformScreenSpaceTex(shookUV);
        #endif

        float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, shookUV);
        return col;
    }

ENDHLSL

SubShader
{
    Cull Off ZWrite Off ZTest Always

    Pass
    {
        HLSLPROGRAM

        #pragma vertex VertDefault
        #pragma fragment frag

        ENDHLSL
    }
}
}
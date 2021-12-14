Shader "Hidden/DownSample/ColorDepthCopy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MyDepthTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        //ZWrite On
        ZTest LEqual
        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "BlitColorDepth"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_DownSampleTex);
            SAMPLER(sampler_DownSampleTex);
            
            half4 Fragment(Varyings input, out float outDepth : SV_Depth) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = SAMPLE_TEXTURE2D_X(_DownSampleTex, sampler_DownSampleTex, input.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_DownSampleTex, sampler_DownSampleTex, input.uv);
                outDepth = depth;

                #ifdef _LINEAR_TO_SRGB_CONVERSION
                color = LinearToSRGB(color);
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}
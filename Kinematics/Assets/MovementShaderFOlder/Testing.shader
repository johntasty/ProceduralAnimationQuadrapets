Shader "Universal Render Pipeline/UnlitShit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

            // BlendMode
            [HideInInspector] _Surface("__surface", Float) = 0.0
            [HideInInspector] _Blend("__blend", Float) = 0.0
            [HideInInspector] _AlphaClip("__clip", Float) = 0.0
            [HideInInspector] _SrcBlend("Src", Float) = 1.0
            [HideInInspector] _DstBlend("Dst", Float) = 0.0
            [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
            [HideInInspector] _Cull("__cull", Float) = 2.0

            // Editmode props
            [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

            // ObsoleteProperties
            [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
            [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
            [HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit
    }
        SubShader
            {
                Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
                LOD 100

                Blend[_SrcBlend][_DstBlend]
                ZWrite[_ZWrite]
                Cull[_Cull]

                Pass
                {
                    Name "Unlit"
                    HLSLPROGRAM
                // Required to compile gles 2.0 with standard srp library
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x

                #pragma vertex vert
                #pragma fragment frag
                #pragma shader_feature _ALPHATEST_ON
                #pragma shader_feature _ALPHAPREMULTIPLY_ON

                // -------------------------------------
                // Unity defined keywords
                #pragma multi_compile_fog
                #pragma multi_compile_instancing
                #pragma instancing_options procedural:ConfigureProcedural
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
                half _Glossiness;
                half _Metallic;

                struct Attributes
                {
                    float4 positionOS       : POSITION;
                    float2 uv               : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float2 uv        : TEXCOORD0;
                    float fogCoord : TEXCOORD1;
                    float4 vertex : SV_POSITION;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };


#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<float4> positionBuffer;

#endif
                void ConfigureProcedural()
                {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                    /// Positions are calculated in the compute shader.
                    /// here we just use them.
                    float4 position = positionBuffer[unity_InstanceID];
                    float scale = position.w;

                    unity_ObjectToWorld._11_21_31_41 = float4(scale, 0, 0, 0);
                    unity_ObjectToWorld._12_22_32_42 = float4(0, scale, 0, 0);
                    unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale, 0);
                    unity_ObjectToWorld._14_24_34_44 = float4(position.xyz, 1);
                    unity_WorldToObject = unity_ObjectToWorld;
                    unity_WorldToObject._14_24_34 *= -1;
                    unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
                }

                Varyings vert(Attributes input)
                {
                    Varyings output = (Varyings)0;

                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                    output.vertex = vertexInput.positionCS;
                    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    half2 uv = input.uv;
                    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                    half3 color = texColor.rgb * _BaseColor.rgb;
                    half alpha = texColor.a * _BaseColor.a;
                    AlphaDiscard(alpha, _Cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
                    color *= alpha;
    #endif

                    color = MixFog(color, input.fogCoord);

                    return half4(color, alpha);
                }
                ENDHLSL
            }



            }
                FallBack "Hidden/Universal Render Pipeline/FallbackError"
                    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}
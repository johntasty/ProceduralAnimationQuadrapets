Shader "Unlit/MovementShader"
{
    Properties
    {
            _BaseMap("Texture", 2D) = "white" {}
            _BaseColor("Color", Color) = (1, 1, 1, 1)
            _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

            _EffectRadius("Wave Effect Radius",Range(0.0,1.0)) = 0.5
             _WaveSpeed("Wave Speed", Range(0.0,100.0)) = 3.0
             _WaveHeight("Wave Height", Range(0.0,60.0)) = 5.0
             _WaveDensity("Wave Density", Range(0.0001,1.0)) = 0.007
             _Yoffset("Y Offset",Float) = 0.0
             _Zoffset("Z Offset",Float) = 0.0
             _Xoffset("X Offset",Float) = 0.0
             _Threshold("Threshold",Range(0,30)) = 3
             _StrideSpeed("Stride Speed",Range(0.0,10.0)) = 2.0
             _StrideStrength("Stride Strength", Range(0.0,20.0)) = 3.0
             _MoveOffset("Move Offset",Float) = 0.0

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
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _SpecColor;
                half4 _EmissionColor;
                half _Cutoff;
                half _Glossiness;
                half _Metallic;
                half _StrideSpeed;
                half _StrideStrength;
                half _MoveOffset;
                half _EffectRadius;
                half _WaveSpeed;
                half _WaveHeight;
                half _WaveDensity;
                half _Yoffset;
                half _Zoffset;
                half _Xoffset;
                int _Threshold;
                
                TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
                half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
                {
                    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
                #ifdef _SPECGLOSSMAP
                    specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
                #elif defined(_SPECULAR_COLOR)
                    specularSmoothness = specColor;
                #endif

                #ifdef _GLOSSINESS_FROM_BASE_ALPHA
                    specularSmoothness.a = exp2(10 * alpha + 1);
                #else
                    specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
                #endif

                    return specularSmoothness;
                }

                
              
                
                struct Attributes
                {
                    float4 positionOS       : POSITION;
                    float4 color            : COLOR;
                    float2 uv               : TEXCOORD0;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float2 uv        : TEXCOORD0;
                    float4 color     : COLOR;
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
                

                    
                    //half yValue = input.positionOS.y - _Yoffset;
                    //half zValue = input.positionOS.z - _Zoffset;
                    //half xValue = input.positionOS.x - _Xoffset;
                    //half sinUse = sin(-_Time.y * _WaveSpeed + _MoveOffset + float2(xValue, yValue) * _WaveDensity);
                    //half yDirScaling = xValue;//clamp(pow(xValue * _EffectRadius, _Threshold), 0.0, 1.0);

                    //input.positionOS.y = input.positionOS.y + sinUse * _WaveHeight * (yDirScaling * xValue);
                    //input.positionOS.y = input.positionOS.y + sin(-_Time.y * _StrideSpeed + _MoveOffset) * _StrideStrength;
                    //input.positionOS.xyz += movePos;

                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                   
                   
                    output.vertex = (vertexInput).positionCS;
                    
                    output.color = input.color;
                    
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
                    half3 color = texColor.rgb ;
                    half alpha = texColor.a * _BaseColor.a;
                    AlphaDiscard(alpha, _Cutoff);

                #ifdef _ALPHAPREMULTIPLY_ON
                    color *= alpha;
                #endif

                    color = MixFog(color, input.fogCoord);
                    
                    return float4(color, alpha);
                }
                ENDHLSL
            }
           
                   
    }
                FallBack "Hidden/Universal Render Pipeline/FallbackError"
                    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}

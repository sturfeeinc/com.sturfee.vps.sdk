Shader "Sturfee/URP-BuildingHide"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0.35,0.4,0.45,1.0)
    }
 
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Geometry-1"
        }
 
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
 
            //Blend DstColor Zero, Zero One
            //Cull Back
            //ZTest LEqual
            //ZWrite Off

            Blend Zero SrcColor		
		    Offset 0, -1
		    ZWrite On
   
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
 
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 
            CBUFFER_START(UnityPerMaterial)
            float4 _ShadowColor;
            CBUFFER_END
 
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct Varyings
            {
                float4 positionCS               : SV_POSITION;
                float3 positionWS               : TEXCOORD0;
                float fogCoord                  : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
 
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
 
                return output;
            }
 
            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
 
                half4 color = half4(1,1,1,1);
 
            #ifdef _MAIN_LIGHT_SHADOWS
                VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                vertexInput.positionWS = input.positionWS;
 
                float4 shadowCoord = GetShadowCoord(vertexInput);
                half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
                color = lerp(half4(1,1,1,1), _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
                color.rgb = MixFogColor(color.rgb, half3(1,1,1), input.fogCoord);
            #else
                Light mainLight = GetMainLight();
            #endif


            // Additional lights loop
            #ifdef _ADDITIONAL_LIGHTS
                // Returns the amount of lights affecting the object being renderer.
                // These lights are culled per-object in the forward renderer
                half3 lightingFromAdditionalLights = half3(0,0,0);
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    // Similar to GetMainLight, but it takes a for-loop index. This figures out the
                    // per-object light index and samples the light buffer accordingly to initialized the
                    // Light struct. If _ADDITIONAL_LIGHT_SHADOWS is defined it will also compute shadows.
                    //Light light = GetAdditionalLight(i, IN.positionWS, positionWS);
                    Light light = GetAdditionalLight(i, input.positionWS, half4(1, 1, 1, 1));

                    // Same functions used to shade the main light.
                    //color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
                    //color += light.color;

                    half shadowAttenutation = light.shadowAttenuation;
                    color = lerp(half4(1,1,1,1), _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
                    color.rgb = MixFogColor(color.rgb, half3(1,1,1), input.fogCoord);
                }
            #endif

                return color;
            }
 
            ENDHLSL
        }

        //// Used for rendering shadowmaps
        //UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }


    FallBack "Hidden/InternalErrorShader"
}
Shader "Custom/Triplanar World Space"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1,1,1,1)

        _Tiling ("Tiling (per meter)", Float) = 1
        _BlendSharpness ("Blend Sharpness", Range(1,20)) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;
                float _Tiling;
                float _BlendSharpness;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);

                return output;
            }


            float3 CalculateTriplanarWeights(float3 normal)
            {
                float3 weights = abs(normal);

                weights = pow(weights, _BlendSharpness);

                float weightSum =
                    weights.x +
                    weights.y +
                    weights.z;

                return weights / max(weightSum, 0.0001);
            }


            float4 SampleTriplanar(
                float3 positionWS,
                float3 normalWS
            )
            {
                float3 weights =
                    CalculateTriplanarWeights(normalWS);


                // X axis projection
                // Uses YZ coordinates
                float2 uvX =
                    positionWS.zy * _Tiling;


                // Y axis projection
                // Uses XZ coordinates
                float2 uvY =
                    positionWS.xz * _Tiling;


                // Z axis projection
                // Uses XY coordinates
                float2 uvZ =
                    positionWS.xy * _Tiling;


                float4 sampleX =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        uvX
                    );


                float4 sampleY =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        uvY
                    );


                float4 sampleZ =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        uvZ
                    );


                return
                    sampleX * weights.x +
                    sampleY * weights.y +
                    sampleZ * weights.z;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS =
                    normalize(input.normalWS);


                float4 sampledTexture =
                    SampleTriplanar(
                        input.positionWS,
                        normalWS
                    );


                float3 albedo =
                    sampledTexture.rgb *
                    _BaseColor.rgb;


                Light mainLight =
                    GetMainLight();


                float NdotL =
                    saturate(
                        dot(
                            normalWS,
                            mainLight.direction
                        )
                    );


                float3 lighting =
                    mainLight.color *
                    NdotL;


                float3 finalColor =
                    albedo *
                    (
                        lighting +
                        0.25
                    );


                return half4(
                    finalColor,
                    1
                );
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
Shader "Universal Render Pipeline/PSX_Terrain"
{
    Properties
    {
        //PSX
        _resolution("Target Resolution", Vector) = (426, 240, 0, 0)
        _DitherStrenght("DitherStrenght", Float) = 0
        _DitherStrenghtColor("DitherStrenghtColor", Float) = 0

        [HideInInspector] [ToggleUI] _EnableHeightBlend("EnableHeightBlend", Float) = 0.0
        _HeightTransition("Height Transition", Range(0, 1.0)) = 0.0
        // Layer count is passed down to guide height-blend enable/disable, due
        // to the fact that heigh-based blend will be broken with multipass.
        [HideInInspector][PerRendererData] _NumLayersCount("Total Layer Count", Float) = 1.0

        // set by terrain engine
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector] _Mask3("Mask 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Mask2("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask1("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask0("Mask 0 (R)", 2D) = "grey" {}
        [HideInInspector][Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [HideInInspector][Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 0.5



            // used in fallback on old cards & base map
            [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "grey" {}
            [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)

            [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}

            [ToggleUI] _EnableInstancedPerPixelNormal("Enable Instanced per-pixel normal", Float) = 1.0
    }

        HLSLINCLUDE

#pragma multi_compile_fragment __ _ALPHATEST_ON

                ENDHLSL

                SubShader
            {
                Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "False" "TerrainCompatible" = "True"}

                Pass
                {
                    Name "ForwardLit"
                    Tags { "LightMode" = "UniversalForward" }
                    HLSLPROGRAM
                    #pragma target 3.0

                    #pragma vertex SplatmapVert
                    #pragma fragment SplatmapFragment

                    #define _METALLICSPECGLOSSMAP 1
                    #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

                // -------------------------------------
                // Universal Pipeline keywords
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
                #pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
                #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
                #pragma multi_compile_fragment _ _LIGHT_LAYERS
                #pragma multi_compile_fragment _ _LIGHT_COOKIES
                #pragma multi_compile _ _FORWARD_PLUS
                #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

                // -------------------------------------
                // Unity defined keywords
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile_fog
                #pragma multi_compile_fragment _ DEBUG_DISPLAY
                #pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                #pragma shader_feature_local_fragment _TERRAIN_BLEND_HEIGHT
                #pragma shader_feature_local _NORMALMAP
                #pragma shader_feature_local_fragment _MASKMAP
                // Sample normal in pixel shader when doing instancing
                #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL

                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                ENDHLSL

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

                            sampler2D _MainTex;
                            float4 _MainTex_ST;
                            float2 _resolution;
                            float _DitherStrenght;
                            float _DitherStrenghtColor;

                            v2f vert(appdata v)
                            {
                                v2f o;
                                o.vertex = UnityObjectToClipPos(v.vertex);
                                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                                float2 grid = _resolution.xy * 0.5;
                                float4 snapped = o.vertex;
                                float4 vertexClip = snapped;
                                snapped.xyz = vertexClip.xyz / vertexClip.w;
                                snapped.xy = floor(grid * snapped.xy) / grid;  // This is actual grid
                                snapped.xyz *= vertexClip.w;

                                o.vertex = snapped;

                                return o;
                            }

                            half4 Unity_Dither_half4(half4 In, half4 ScreenPosition)
                            {
                                half2 uv = ScreenPosition.xy * _ScreenParams.xy;
                                half DITHER_THRESHOLDS[16] =
                                {
                                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                                };
                                uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
                                return In - DITHER_THRESHOLDS[index];
                            }

                            fixed4 frag(v2f i) : SV_Target
                            {
                                float4 col = tex2D(_MainTex, i.uv);
                                
                                half4 screenPosition = (half4)ComputeScreenPos(i.vertex);
  
                                half4 dither = Unity_Dither_half4(col, screenPosition * _DitherStrenght.xxxx);

                                float4 finalColor = dither * _DitherStrenghtColor.xxxx;

                                col += finalColor;

                                return col;
                            }
            ENDCG
            }

            Pass
            {
                Name "ShadowCaster"
                Tags{"LightMode" = "ShadowCaster"}

                ZWrite On
                ColorMask 0

                HLSLPROGRAM
                #pragma target 2.0

                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                #pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                // -------------------------------------
                // Universal Pipeline keywords

                // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
                #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                ENDHLSL
            }

            Pass
            {
                Name "GBuffer"
                Tags{"LightMode" = "UniversalGBuffer"}

                HLSLPROGRAM
                #pragma target 4.5

                // Deferred Rendering Path does not support the OpenGL-based graphics API:
                // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
                #pragma exclude_renderers gles3 glcore

                #pragma vertex SplatmapVert
                #pragma fragment SplatmapFragment

                #define _METALLICSPECGLOSSMAP 1
                #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

                // -------------------------------------
                // Universal Pipeline keywords
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
                #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
                #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

                // -------------------------------------
                // Unity defined keywords
                #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
                #pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
                #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

                //#pragma multi_compile_fog
                #pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                #pragma shader_feature_local _TERRAIN_BLEND_HEIGHT
                #pragma shader_feature_local _NORMALMAP
                #pragma shader_feature_local _MASKMAP
                // Sample normal in pixel shader when doing instancing
                #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL
                #define TERRAIN_GBUFFER 1

                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                ENDHLSL
            }

            Pass
            {
                Name "DepthOnly"
                Tags{"LightMode" = "DepthOnly"}

                ZWrite On
                ColorMask R

                HLSLPROGRAM
                #pragma target 2.0

                #pragma vertex DepthOnlyVertex
                #pragma fragment DepthOnlyFragment

                #pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                ENDHLSL
            }

                // This pass is used when drawing to a _CameraNormalsTexture texture
                Pass
                {
                    Name "DepthNormals"
                    Tags{"LightMode" = "DepthNormals"}

                    ZWrite On

                    HLSLPROGRAM
                    #pragma target 2.0
                    #pragma vertex DepthNormalOnlyVertex
                    #pragma fragment DepthNormalOnlyFragment

                    #pragma shader_feature_local _NORMALMAP
                    #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

                    #pragma multi_compile_instancing
                    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitDepthNormalsPass.hlsl"
                    ENDHLSL
                }

                Pass
                {
                    Name "SceneSelectionPass"
                    Tags { "LightMode" = "SceneSelectionPass" }

                    HLSLPROGRAM
                    #pragma target 2.0

                    #pragma vertex DepthOnlyVertex
                    #pragma fragment DepthOnlyFragment

                    #pragma multi_compile_instancing
                    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                    #define SCENESELECTIONPASS
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                    ENDHLSL
                }

                // This pass it not used during regular rendering, only for lightmap baking.
                Pass
                {
                    Name "Meta"
                    Tags{"LightMode" = "Meta"}

                    Cull Off

                    HLSLPROGRAM
                    #pragma vertex TerrainVertexMeta
                    #pragma fragment TerrainFragmentMeta

                    #pragma multi_compile_instancing
                    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
                    #pragma shader_feature EDITOR_VISUALIZATION
                    #define _METALLICSPECGLOSSMAP 1
                    #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitMetaPass.hlsl"

                    ENDHLSL
                }



                UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
            }
                Dependency "AddPassShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Add Pass)"
                Dependency "BaseMapShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Base Pass)"
                Dependency "BaseMapGenShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Basemap Gen)"

                CustomEditor "UnityEditor.Rendering.Universal.TerrainLitShaderGUI"

                Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Include material common properties names
using static UnityEngine.Rendering.HighDefinition.HDMaterialProperties;

namespace UnityEditor.Rendering.HighDefinition
{
    /// <summary>
    /// GUI for HDRP unlit shaders (does not include shader graphs)
    /// </summary>
    class UnlitGUI : HDShaderGUI
    {
        MaterialUIBlockList uiBlocks = new MaterialUIBlockList
        {
            new SurfaceOptionUIBlock(MaterialUIBlock.Expandable.Base, features: SurfaceOptionUIBlock.Features.Unlit),
            new UnlitSurfaceInputsUIBlock(MaterialUIBlock.Expandable.Input),
            new TransparencyUIBlock(MaterialUIBlock.Expandable.Transparency),
            new EmissionUIBlock(MaterialUIBlock.Expandable.Emissive),
            new AdvancedOptionsUIBlock(MaterialUIBlock.Expandable.Advance, AdvancedOptionsUIBlock.Features.Instancing | AdvancedOptionsUIBlock.Features.AddPrecomputedVelocity)
        };

        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                uiBlocks.OnGUI(materialEditor, props);

                // Apply material keywords and pass:
                if (changed.changed)
                {
                    foreach (var material in uiBlocks.materials)
                        SetupMaterialKeywordsAndPassInternal(material);
                }
            }
        }

        protected override void SetupMaterialKeywordsAndPassInternal(Material material) => SetupUnlitMaterialKeywordsAndPass(material);

        // All Setup Keyword functions must be static. It allow to create script to automatically update the shaders with a script if code change
        public static void SetupUnlitMaterialKeywordsAndPass(Material material)
        {
            material.SetupBaseUnlitKeywords();
            material.SetupBaseUnlitPass();

            if (material.HasProperty(kEmissiveColorMap))
                CoreUtils.SetKeyword(material, "_EMISSIVE_COLOR_MAP", material.GetTexture(kEmissiveColorMap));

            // Stencil usage rules:
            // StencilBeforeTransparent and DecalsForwardOutputNormalBuffer need to be tagged during depth prepass
            // RequiresDeferredLighting need to be tagged during GBuffer
            // SubsurfaceScattering need to be tagged during either GBuffer or Forward pass
            // ObjectVelocity need to be tagged in velocity pass.
            // As velocity pass can be use as a replacement of depth prepass it also need to have StencilBeforeTransparent and DecalsForwardOutputNormalBuffer
            // As GBuffer pass can have no depth prepass, it also need to have StencilBeforeTransparent and DecalsForwardOutputNormalBuffer
            // Object velocity is always render after a full depth buffer (if there is no depth prepass for GBuffer all object motion vectors are render after GBuffer)
            // so we have a guarantee than when we write object velocity no other object will be draw on top (and so would have require to overwrite velocity).
            // Final combination is:
            // Prepass: StencilBeforeTransparent,  DecalsForwardOutputNormalBuffer
            // Motion vectors: StencilBeforeTransparent,  DecalsForwardOutputNormalBuffer, ObjectVelocity
            // Forward: LightingMask

            int stencilRef = (int)StencilBeforeTransparent.Clear;
            int stencilWriteMask = (int)StencilBeforeTransparent.RequiresDeferredLighting | (int)StencilBeforeTransparent.SubsurfaceScattering;
            int stencilRefDepth = (int)StencilBeforeTransparent.TraceReflectionRay;
            int stencilWriteMaskDepth = (int)StencilBeforeTransparent.TraceReflectionRay | (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;
            int stencilRefMV = (int)StencilBeforeTransparent.ObjectMotionVector | (int)StencilBeforeTransparent.TraceReflectionRay;
            int stencilWriteMaskMV = (int)StencilBeforeTransparent.ObjectMotionVector | (int)StencilBeforeTransparent.TraceReflectionRay | (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;

            // As we tag both during velocity pass and Gbuffer pass we need a separate state and we need to use the write mask
            material.SetInt(kStencilRef, stencilRef);
            material.SetInt(kStencilWriteMask, stencilWriteMask);
            material.SetInt(kStencilRefDepth, stencilRefDepth);
            material.SetInt(kStencilWriteMaskDepth, stencilWriteMaskDepth);
            material.SetInt(kStencilRefMV, stencilRefMV);
            material.SetInt(kStencilWriteMaskMV, stencilWriteMaskMV);
            material.SetInt(kStencilRefDistortionVec, (int)StencilAfterOpaque.DistortionVectors);
            material.SetInt(kStencilWriteMaskDistortionVec, (int)StencilAfterOpaque.DistortionVectors);
            if (material.HasProperty(kAddPrecomputedVelocity))
            {
                CoreUtils.SetKeyword(material, "_ADD_PRECOMPUTED_VELOCITY", material.GetInt(kAddPrecomputedVelocity) != 0);
            }

        }
    }
} // namespace UnityEditor

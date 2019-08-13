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

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
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

            if (material.HasProperty(kAddPrecomputedVelocity))
            {
                CoreUtils.SetKeyword(material, "_ADD_PRECOMPUTED_VELOCITY", material.GetInt(kAddPrecomputedVelocity) != 0);
            }

            // Set up the stencil state.
            // 0 disables the stencil test.
            int stencilRef       = 0;
            int stencilReadMask  = 0;
            int stencilWriteMask = 0;

            if (material.GetSurfaceType() == SurfaceType.Opaque)
            {
                stencilRef       |= (int)HDRenderPipeline.StencilMaterialFeatures.Forward; // Unlit is forward-only
                stencilReadMask  |= (int)HDRenderPipeline.StencilUsageBeforeTransparent.MaxValue;
                stencilWriteMask |= (int)HDRenderPipeline.StencilUsageBeforeTransparent.MaxValue;
            }
            else // SurfaceType.Transparent
            {
                // Distortion must be able to write to the stencil buffer, but does not need to read it.
                if (material.GetShaderPassEnabled(HDShaderPassNames.s_DistortionVectorsStr))
                {
                    stencilRef       |= (int)HDRenderPipeline.StencilUsageAfterTransparent.DistortionVector;
                    stencilWriteMask |= (int)HDRenderPipeline.StencilUsageAfterTransparent.DistortionVector;
                }
            }

            // Motion vectors are supported by all surface types.
            if (material.GetShaderPassEnabled(HDShaderPassNames.s_MotionVectorsStr))
            {
                // The location of this bit is persistent (before and after transparent).
                stencilRef       |= (int)HDRenderPipeline.StencilUsageBeforeTransparent.ObjectMotionVector;
                stencilWriteMask |= (int)HDRenderPipeline.StencilUsageBeforeTransparent.ObjectMotionVector;
            }

            material.SetInt(kStencilRef,       stencilRef);
            material.SetInt(kStencilReadMask,  stencilReadMask);
            material.SetInt(kStencilWriteMask, stencilWriteMask);
        }
    }
} // namespace UnityEditor

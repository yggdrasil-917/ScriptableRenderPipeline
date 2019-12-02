using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.Rendering.HighDefinition.ShaderGraph
{
    class HDSpeedTreeTarget : ITargetImplementation
    {
        public Type targetType => typeof(SpeedTreeTarget);
        public string displayName => "HDRP";
        public string passTemplatePath => string.Empty;
        public string sharedTemplateDirectory => $"{HDUtils.GetHDRenderPipelinePath()}Editor/ShaderGraph/Templates";

        public static KeywordDescriptor SpeedTreeVersion = new KeywordDescriptor()
        {
            displayName = "SpeedTree Asset Version",
            referenceName = "SPEEDTREE_",
            type = KeywordType.Enum,
            definition = KeywordDefinition.ShaderFeature,
            scope = KeywordScope.Local,
            entries = new KeywordEntry[]
            {
                    new KeywordEntry() { displayName = "Version 7", referenceName = "V7" },
                    new KeywordEntry() { displayName = "Version 8", referenceName = "V8" },
            }
        };

        public static KeywordDescriptor LodFadePercentage = new KeywordDescriptor()
        {
            displayName = "LOD Fade Percentage",
            referenceName = "LOD_FADE_PERCENTAGE",
            type = KeywordType.Boolean,
            definition = KeywordDefinition.MultiCompile,
            scope = KeywordScope.Local,
        };

        public bool IsValid(IMasterNode masterNode)
        {
            return (masterNode is PBRMasterNode ||
                    masterNode is UnlitMasterNode ||
                    masterNode is HDUnlitMasterNode ||
                    masterNode is HDLitMasterNode ||
                    masterNode is FabricMasterNode);
        }
        public bool IsPipelineCompatible(RenderPipelineAsset currentPipeline)
        {
            return currentPipeline is HDRenderPipelineAsset;
        }

        private SubShaderDescriptor UpdateSubShader(ref SubShaderDescriptor origDescriptor)
        {
            SubShaderDescriptor modDescriptor = origDescriptor;

            modDescriptor.renderQueueOverride = "AlphaTest";

            foreach(PassCollection.Item p in modDescriptor.passes)
            {
                p.descriptor.keywords.Add(SpeedTreeVersion);
                p.descriptor.keywords.Add(LodFadePercentage);
            }

            return modDescriptor;
        }

        public void SetupTarget(ref TargetSetupContext context)
        {
            context.AddAssetDependencyPath(AssetDatabase.GUIDToAssetPath("4592b595eeb00ee42868a87a4901d29b")); // SpeedTreeTarget
            context.AddAssetDependencyPath(AssetDatabase.GUIDToAssetPath("e0988759073f96945ba34b15eed233e0")); // HDSpeedTreeTarget

            switch (context.masterNode)
            {
                case PBRMasterNode pbrMasterNode:
                    context.SetupSubShader(HDSubShaders.PBR);
                    break;
                case UnlitMasterNode unlitMasterNode:
                    context.SetupSubShader(HDSubShaders.Unlit);
                    break;
                case HDUnlitMasterNode hdUnlitMasterNode:
                    context.SetupSubShader(HDSubShaders.HDUnlit);
                    break;
                case HDLitMasterNode hdLitMasterNode:
                default:
                    context.SetupSubShader(UpdateSubShader(ref HDSubShaders.HDLit));
                    break;
                case FabricMasterNode fabricMasterNode:
                    context.SetupSubShader(HDSubShaders.Fabric);
                    break;
            }
        }
    }
}

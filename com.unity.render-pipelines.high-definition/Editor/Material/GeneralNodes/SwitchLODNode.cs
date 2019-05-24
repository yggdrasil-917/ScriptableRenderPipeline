using System.Reflection;
using UnityEditor.Graphing;
using UnityEngine;
using UnityEditor.ShaderGraph;
using System.Collections.Generic;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    [Title("Math", "HDRP/Material", "Switch LOD")]
    class SwitchLODNode : AbstractMaterialNode, IGeneratesBodyCode, IGenerateMultiCompile
    {
        List<MaterialSlot> m_TempSlots = new List<MaterialSlot>();

        public const int OutputSlotId = 0;
        public const int InputIdLow = 1;
        public const int InputIdMed = 2;
        public const int InputIdHigh = 3;

        const string kOutputSlotName = "Out";
        const string kInputIdLowName = "Low";
        const string kInputIdMedName = "Med";
        const string kInputIdHighName = "High";
        const string kKeywordLow = "HDRP_MATERIAL_LOD_LOW";
        const string kKeywordHigh = "HDRP_MATERIAL_LOD_HIGH";

        public override bool hasPreview { get { return true; } }

        public SwitchLODNode()
        {
            name = "Switch LOD";
            UpdateNodeAfterDeserialization();
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new DynamicVectorMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, Vector4.zero, ShaderStageCapability.All));
            AddSlot(new DynamicVectorMaterialSlot(InputIdLow, kInputIdLowName, kInputIdLowName, SlotType.Input, Vector4.zero, ShaderStageCapability.All));
            AddSlot(new DynamicVectorMaterialSlot(InputIdMed, kInputIdMedName, kInputIdMedName, SlotType.Input, Vector4.zero, ShaderStageCapability.All));
            AddSlot(new DynamicVectorMaterialSlot(InputIdHigh, kInputIdHighName, kInputIdHighName, SlotType.Input, Vector4.zero, ShaderStageCapability.All));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId, InputIdLow, InputIdMed, InputIdHigh });
        }

        // Node generations
        public virtual void GenerateNodeCode(ShaderGenerator visitor, GraphContext graphContext, GenerationMode generationMode)
        {
            var outputName = GetVariableNameForSlot(OutputSlotId);
            var lowValue = GetSlotValue(InputIdLow, generationMode);
            var medValue = GetSlotValue(InputIdMed, generationMode);
            var highValue = GetSlotValue(InputIdHigh, generationMode);

            m_TempSlots.Clear();
            GetOutputSlots(m_TempSlots);
            var slot = m_TempSlots[0];

            var outputType = NodeUtils.ConvertConcreteSlotValueTypeToString(precision, slot.concreteValueType);

            var result = $@"
#if HDRP_MATERIAL_LOD_LOW
    {outputType} {outputName} = {lowValue};
#elif HDRP_MATERIAL_LOD_HIGH
    {outputType} {outputName} = {highValue};
#else
    {outputType} {outputName} = {medValue};
#endif
";
            visitor.AddShaderChunk(result, true);
        }

        public void GetMultiCompiles(HashSetAllocator setAllocator, List<HashSet<string>> values)
        {
            var multiCompiles = setAllocator();
            multiCompiles.Add("_");
            multiCompiles.Add(kKeywordLow);
            multiCompiles.Add(kKeywordHigh);
            values.Add(multiCompiles);
        }

        public void GetInputSlotsFor(HashSet<string> keywordSet, List<int> slotIds)
        {
            if (keywordSet.Contains(kKeywordHigh))
                slotIds.Add(InputIdHigh);
            else if (keywordSet.Contains(kKeywordLow))
                slotIds.Add(InputIdLow);
            else
                slotIds.Add(InputIdMed);
        }
    }
}

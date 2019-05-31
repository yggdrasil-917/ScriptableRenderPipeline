using System.Reflection;
using UnityEditor.Graphing;
using UnityEngine;
using UnityEditor.ShaderGraph;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph.Drawing;
using System;
using UnityEditor.Graphing.Util;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    enum SwitchLOD
    {
        Low,
        Medium,
        High
    }

    [Title("Math", "HDRP/Material", "Switch LOD")]
    class SwitchLODNode : AbstractMaterialNode, IGeneratesBodyCode, IGenerateMultiCompile, IHasSettings
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
        const string kKeywordLow = "MATERIAL_QUALITY_LOW";
        const string kKeywordHigh = "MATERIAL_QUALITY_HIGH";

        public override bool hasPreview { get { return true; } }

        // TODO: the previewed LOD should be a propertty of the graph, not of the node
        internal SwitchLOD previewedLOD;

        public SwitchLODNode()
        {
            name = "Switch LOD";
            UpdateNodeAfterDeserialization();
        }

        public VisualElement CreateSettingsElement()
            => new SwitchLODSettingsView(this);

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


            string result = $@"
#if MATERIAL_QUALITY_LOW
    {outputType} {outputName} = {lowValue};
#elif MATERIAL_QUALITY_HIGH
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

    class SwitchLODSettingsView : VisualElement
    {
        SwitchLODNode m_Node;

        public SwitchLODSettingsView(SwitchLODNode node)
        {
            m_Node = node;
            var ps = new PropertySheet();
            ps.Add(new PropertyRow(new Label("LOD Preview")), row =>
            {
                row.Add(new EnumField(SwitchLOD.Medium), field =>
                {
                    field.value = m_Node.previewedLOD;
                    field.RegisterValueChangedCallback(ChangeLOD);
                });
            });
            Add(ps);
        }

        void ChangeLOD(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.previewedLOD, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Previewed LOD");
            m_Node.Dirty(ModificationScope.Graph);
            m_Node.previewedLOD = (SwitchLOD)evt.newValue;
        }
    }
}

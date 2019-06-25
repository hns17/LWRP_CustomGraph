using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;


/**
    @file   LegacyMasterNode.cs
    @date   2019.06.05
    @author hns17(hns17.tistory.com)
    @brief  Legacy용 MasterNode를 생성한다.
            Shader에 연결될 레이아웃을 정의하고 ShaderGraph에 표현된다.
*/

namespace Hns17.CustomNode
{
    [Serializable]
    [Title("Master", "Legacy")]
    class LegacyMasterNode : MasterNode<ILegaySubShader>, IMayRequirePosition, IMayRequireNormal
    {
        public const string DiffuseSlotName = "Diffuse";
        public const string NormalSlotName = "Normal";
        public const string EmissionSlotName = "Emission";
        public const string SmoothnessSlotName = "Smoothness";
        public const string AlphaSlotName = "Alpha";
        public const string AlphaClipThresholdSlotName = "AlphaClipThreshold";
        public const string CoefficientSlotName = "Coefficient";
        public const string SpecularGlossSlotName = "SpecularGloss";
        public const string PositionName = "Position";

        public const int DiffuseSlotId = 0;
        public const int NormalSlotId = 1;
        public const int EmissionSlotId = 4;
        public const int SmoothnessSlotId = 5;
        public const int AlphaSlotId = 7;
        public const int AlphaThresholdSlotId = 8;
        public const int CoefficientSlotId = 2;
        public const int SpecularGlossSlotId = 3;
        public const int PositionSlotId = 9;


        public enum ShadeType
        {
            Lambert, HalfLambert, BlinnPhong
        }

        [SerializeField]
        ShadeType m_ShadeType;

        public ShadeType shadeType {
            get { return m_ShadeType; }
            set
            {
                if (m_ShadeType == value)
                    return;
                m_ShadeType = value;
                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Topological);
            }
        }

        [SerializeField]
        SurfaceType m_SurfaceType;

        public SurfaceType surfaceType
        {
            get { return m_SurfaceType; }
            set
            {
                if (m_SurfaceType == value)
                    return;

                m_SurfaceType = value;
                Dirty(ModificationScope.Graph);
            }
        }

        [SerializeField]
        AlphaMode m_AlphaMode;

        public AlphaMode alphaMode
        {
            get { return m_AlphaMode; }
            set
            {
                if (m_AlphaMode == value)
                    return;

                m_AlphaMode = value;
                Dirty(ModificationScope.Graph);
            }
        }

        [SerializeField]
        bool m_TwoSided;

        public ToggleData twoSided
        {
            get { return new ToggleData(m_TwoSided); }
            set
            {
                if (m_TwoSided == value.isOn)
                    return;
                m_TwoSided = value.isOn;
                Dirty(ModificationScope.Graph);
            }
        }

        public LegacyMasterNode()
        {
            UpdateNodeAfterDeserialization();
        }

        public override string documentationURL
        {
            get { return "https://github.com/Unity-Technologies/ShaderGraph/wiki/PBR-Master-Node"; }
        }


        //@@
        public sealed override void UpdateNodeAfterDeserialization()
        {
            base.UpdateNodeAfterDeserialization();
            name = "Legacy Master";
            AddSlot(new PositionMaterialSlot(PositionSlotId, PositionName, PositionName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new ColorRGBMaterialSlot(DiffuseSlotId, DiffuseSlotName, DiffuseSlotName, SlotType.Input, Color.grey.gamma, ColorMode.Default, ShaderStageCapability.Fragment));
            AddSlot(new NormalMaterialSlot(NormalSlotId, NormalSlotName, NormalSlotName, CoordinateSpace.Tangent, ShaderStageCapability.Fragment));
            AddSlot(new ColorRGBMaterialSlot(EmissionSlotId, EmissionSlotName, EmissionSlotName, SlotType.Input, Color.black, ColorMode.Default, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(AlphaSlotId, AlphaSlotName, AlphaSlotName, SlotType.Input, 1f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(AlphaThresholdSlotId, AlphaClipThresholdSlotName, AlphaClipThresholdSlotName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));

            if(shadeType != ShadeType.Lambert)
            {
                if(shadeType == ShadeType.HalfLambert)
                {
                    AddSlot(new Vector1MaterialSlot(CoefficientSlotId, CoefficientSlotName, CoefficientSlotName, SlotType.Input, 1f, ShaderStageCapability.Fragment));

                    RemoveSlotsNameNotMatching(
                    new[]
                    {
                        PositionSlotId,
                        DiffuseSlotId,
                        NormalSlotId,
                        EmissionSlotId,
                        AlphaSlotId,
                        AlphaThresholdSlotId,
                        CoefficientSlotId
                    }, true);
                }
                else
                {
                    AddSlot(new Vector1MaterialSlot(SmoothnessSlotId, SmoothnessSlotName, SmoothnessSlotName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));
                    AddSlot(new ColorRGBMaterialSlot(SpecularGlossSlotId, SpecularGlossSlotName, SpecularGlossSlotName, SlotType.Input, Color.grey, ColorMode.Default, ShaderStageCapability.Fragment));

                    RemoveSlotsNameNotMatching(
                    new[]
                    {
                        PositionSlotId,
                        DiffuseSlotId,
                        NormalSlotId,
                        EmissionSlotId,
                        AlphaSlotId,
                        AlphaThresholdSlotId,
                        SmoothnessSlotId,
                        SpecularGlossSlotId
                    }, true);
                }
            }
            else
            {
                RemoveSlotsNameNotMatching(
                new[]
                {
                    PositionSlotId,
                    DiffuseSlotId,
                    NormalSlotId,
                    EmissionSlotId,
                    AlphaSlotId,
                    AlphaThresholdSlotId
                }, true);
            }
        }
        
        protected override VisualElement CreateCommonSettingsElement()
        {
            return new LegacySettingView(this);
        }

        public NeededCoordinateSpace RequiresNormal(ShaderStageCapability stageCapability = ShaderStageCapability.All)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequireNormal>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresNormal(stageCapability));
        }

        public NeededCoordinateSpace RequiresPosition(ShaderStageCapability stageCapability = ShaderStageCapability.All)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequirePosition>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresPosition(stageCapability));
        }
    }

}


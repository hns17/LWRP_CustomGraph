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
    @file   CustomLitMasterNode.cs
    @date   2019.06.10
    @author hns17(hns17.tistory.com)
    @brief  CustomLit용 MasterNode를 생성한다.
            Shader에 연결될 레이아웃을 정의하고 ShaderGraph에 표현된다.
*/

namespace Hns17.CustomNode
{
    [Serializable]
    [Title("Master", "CustomLit")]
    class CustomLitMasterNode : MasterNode<ICustomLitSubShader>, IMayRequirePosition, IMayRequireNormal
    {
        public const string ColorSlotName = "Color";
        public const string NormalSlotName = "Normal";
        public const string AlphaSlotName = "Alpha";
        public const string AlphaClipThresholdSlotName = "AlphaClipThreshold";
        public const string PositionName = "Position";
        public const string EmissiveIntensitySlotName = "EmissiveIntensity";

        public const int ColorSlotId = 0;
        public const int NormalSlotId = 1;
        public const int AlphaSlotId = 2;
        public const int AlphaThresholdSlotId = 3;
        public const int PositionSlotId = 4;
        public const int EmissiveIntensitySlotId = 5;
        


        public enum ShadeType { Lit, UnLit}

        [SerializeField]
        public ShadeType m_ShadeType;
        public ShadeType shadeType
        {
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

        public enum CullMode { Back, Front, Off}
        [SerializeField]
        CullMode m_CullMode;

        public CullMode cullMode
        {
            get { return m_CullMode; }
            set
            {
                if (m_CullMode == value)
                    return;

                m_CullMode = value;
                Dirty(ModificationScope.Graph);
            }
        }


        public enum DepthTest { LEqual, Less, Greater, GEqual, Equal, NotEqual, Always }
        [SerializeField]
        DepthTest m_DepthTest;

        public DepthTest depthTest
        {
            get { return m_DepthTest; }
            set
            {
                if (m_DepthTest == value)
                    return;

                m_DepthTest = value;
                Dirty(ModificationScope.Graph);
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

        public CustomLitMasterNode()
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
            name = "CustomLit Master";
            AddSlot(new PositionMaterialSlot(PositionSlotId, PositionName, PositionName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new ColorRGBMaterialSlot(ColorSlotId, ColorSlotName, ColorSlotName, SlotType.Input, Color.grey.gamma, ColorMode.Default, ShaderStageCapability.Fragment));

            bool isLit = m_ShadeType == ShadeType.Lit ? true : false;
            AddSlot(new Vector1MaterialSlot(AlphaSlotId, AlphaSlotName, AlphaSlotName, SlotType.Input, 1f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(AlphaThresholdSlotId, AlphaClipThresholdSlotName, AlphaClipThresholdSlotName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));

            if (isLit) {
                AddSlot(new NormalMaterialSlot(NormalSlotId, NormalSlotName, NormalSlotName, CoordinateSpace.Tangent, ShaderStageCapability.Fragment));
                AddSlot(new Vector1MaterialSlot(EmissiveIntensitySlotId, EmissiveIntensitySlotName, EmissiveIntensitySlotName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));

               RemoveSlotsNameNotMatching(
                   new[]
                   {
                        PositionSlotId,
                        ColorSlotId,
                        AlphaSlotId,
                        AlphaThresholdSlotId,
                        NormalSlotId,
                        EmissiveIntensitySlotId
                   }, true);
            }
            else
            {
                RemoveSlotsNameNotMatching(
                   new[]
                   {
                        PositionSlotId,
                        ColorSlotId,
                        AlphaSlotId,
                        AlphaThresholdSlotId
                   }, true);
            }
        }

        protected override VisualElement CreateCommonSettingsElement()
        {
            return new CustomLitSettingView(this);
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

        public string GetCullMode()
        {
            string strCull;
            switch (m_CullMode)
            {
                case CullMode.Back:
                    strCull = "Cull Back";
                    break;
                case CullMode.Front:
                    strCull = "Cull Front";
                    break;
                case CullMode.Off:
                    strCull = "Cull Off";
                    break;
                default:
                    strCull = "Cull Back";
                    break;
            }
            return strCull;
        }


        public string GetDepthTest()
        {
            string strDepthTest;
            switch (m_DepthTest)
            {
                case DepthTest.Always:
                    strDepthTest = "ZTest Always";
                    break;
                case DepthTest.Equal:
                    strDepthTest = "ZTest Equal";
                    break;
                case DepthTest.GEqual:
                    strDepthTest = "ZTest GEqual";
                    break;
                case DepthTest.Greater:
                    strDepthTest = "ZTest Greater";
                    break;
                case DepthTest.LEqual:
                    strDepthTest = "ZTest LEqual";
                    break;
                case DepthTest.Less:
                    strDepthTest = "ZTest Less";
                    break;
                case DepthTest.NotEqual:
                    strDepthTest = "ZTest NotEqual";
                    break;
                default:
                    strDepthTest = "ZTest Less";
                    break;
            }
            return strDepthTest;
        }
    }
}


using System;
using UnityEditor.Graphing.Util;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;


/**
    @file   LegacySettingView.cs
    @date   2019.06.05
    @author hns17(hns17.tistory.com)
    @brief  LegacyMasterNode의 CreateCommonSettingsElement 함수에서 생성.
            마스터 노드의 속성(마스터 노드 오른쪽 상단 바퀴모양)을 구성. 
*/

namespace Hns17.CustomNode
{
    class LegacySettingView : VisualElement
    {
        LegacyMasterNode m_Node;
        public LegacySettingView(LegacyMasterNode node)
        {
            m_Node = node;

            PropertySheet ps = new PropertySheet();

            ps.Add(new PropertyRow(new Label("Shade Type")), (row) =>
            {
                row.Add(new EnumField(LegacyMasterNode.ShadeType.Lambert), (field) =>
                {
                    field.value = m_Node.shadeType;
                    field.RegisterValueChangedCallback(ChangeShadeType);
                });
            });

            ps.Add(new PropertyRow(new Label("Surface")), (row) =>
            {
                row.Add(new EnumField(SurfaceType.Opaque), (field) =>
                {
                    field.value = m_Node.surfaceType;
                    field.RegisterValueChangedCallback(ChangeSurface);
                });
            });

            ps.Add(new PropertyRow(new Label("Blend")), (row) =>
            {
                row.Add(new EnumField(AlphaMode.Additive), (field) =>
                {
                    field.value = m_Node.alphaMode;
                    field.RegisterValueChangedCallback(ChangeAlphaMode);
                });
            });

            ps.Add(new PropertyRow(new Label("Two Sided")), (row) =>
            {
                row.Add(new Toggle(), (toggle) =>
                {
                    toggle.value = m_Node.twoSided.isOn;
                    toggle.OnToggleChanged(ChangeTwoSided);
                });
            });

            Add(ps);
        }


        void ChangeShadeType(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.shadeType, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Shade Type Change");
            m_Node.shadeType = (LegacyMasterNode.ShadeType)evt.newValue;
        }

        void ChangeSurface(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.surfaceType, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Surface Change");
            m_Node.surfaceType = (SurfaceType)evt.newValue;
        }

        void ChangeAlphaMode(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.alphaMode, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Alpha Mode Change");
            m_Node.alphaMode = (AlphaMode)evt.newValue;
        }

        void ChangeTwoSided(ChangeEvent<bool> evt)
        {
            m_Node.owner.owner.RegisterCompleteObjectUndo("Two Sided Change");
            ToggleData td = m_Node.twoSided;
            td.isOn = evt.newValue;
            m_Node.twoSided = td;
        }
    }
}

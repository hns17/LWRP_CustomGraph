using System;
using UnityEditor.Graphing.Util;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;


/**
    @file   CustomLitSettingView.cs
    @date   2019.06.10
    @author hns17(hns17.tistory.com)
    @brief  CustomLitMasterNode의 CreateCommonSettingsElement 함수에서 생성.
            마스터 노드의 속성(마스터 노드 오른쪽 상단 바퀴모양)을 구성. 
*/

namespace Hns17.CustomNode
{
    class CustomLitSettingView : VisualElement
    {
        CustomLitMasterNode m_Node;
        public CustomLitSettingView(CustomLitMasterNode node)
        {
            m_Node = node;

            PropertySheet ps = new PropertySheet();


            ps.Add(new PropertyRow(new Label("ShadeType")), (row) =>
            {
                row.Add(new EnumField(CustomLitMasterNode.ShadeType.Lit), (field) =>
                {
                    field.value = m_Node.shadeType;
                    field.RegisterValueChangedCallback(ChangeShadeType);
                });
            });

            ps.Add(new PropertyRow(new Label("CullMode")), (row) =>
            {
                row.Add(new EnumField(CustomLitMasterNode.CullMode.Back), (field) =>
                {
                    field.value = m_Node.cullMode;
                    field.RegisterValueChangedCallback(ChangeCullMode);
                });
            });

            ps.Add(new PropertyRow(new Label("DepthTest")), (row) =>
            {
                row.Add(new EnumField(CustomLitMasterNode.DepthTest.LEqual), (field) =>
                {
                    field.value = m_Node.depthTest;
                    field.RegisterValueChangedCallback(ChangeDepthTest);
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
            m_Node.shadeType = (CustomLitMasterNode.ShadeType)evt.newValue;
        }

        void ChangeCullMode(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.cullMode, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Cull Mode Change");
            m_Node.cullMode = (CustomLitMasterNode.CullMode)evt.newValue;
        }

        void ChangeDepthTest(ChangeEvent<Enum> evt)
        {
            if (Equals(m_Node.depthTest, evt.newValue))
                return;

            m_Node.owner.owner.RegisterCompleteObjectUndo("Depth Test Change");
            m_Node.depthTest = (CustomLitMasterNode.DepthTest)evt.newValue;
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

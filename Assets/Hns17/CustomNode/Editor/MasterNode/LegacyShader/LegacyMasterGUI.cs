
using UnityEditor;

/**
    @file   LegacyMasterGUI.cs
    @date   2019.06.05
    @author hns17(hns17.tistory.com)
    @brief  OnGUI를 오버라이딩 하는 것으로 보아 마테리얼의 GUI 업데이트 기능으로 보이는데 정확하게 확인하지 않았다.
            Default Lit Shader를 만들면 SufaceInput 속성에 EmissionMap 관련 속성이 있는데 
            해당 속성을 업데이트 하기위한 용도가 아닌가 싶다.
            일단 PBS MasterNode에 있으니 만들어 두자.

    @ref    https://docs.unity3d.com/ScriptReference/MaterialEditor.LightmapEmissionFlagsProperty.html
*/

namespace Hns17.CustomNode
{
    public class LegacyMasterGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            materialEditor.PropertiesDefaultGUI(props);

            foreach (MaterialProperty prop in props)
            {
                if (prop.name == "_EmissionColor")
                {
                    if (materialEditor.EmissionEnabledProperty())
                    {
                        materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
                    }
                    return;
                }
            }
        }
    }
}

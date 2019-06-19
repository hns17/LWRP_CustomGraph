using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShaderGraph;
using UnityEngine;

/**
    @file   CreateCustomShaderGraph.cs
    @date   2019.06.05
    @author hns17(hns17.tistory.com)
    @brief  Unity의 Menu에 Legacy 항목 생성 및 ShaderGraph Asset 추가
            
*/
namespace Hns17.CustomNode
{
    public class CreateCustomShaderGraph : EndNameEditAction
    {
        [MenuItem("Assets/Create/Shader/Hns17/Legacy",false, 208)]
        public static void CreateLegacyMaterialGraph()
        {  
            var graph = CreateInstance<CreateCustomShaderGraph>();
            var format = string.Format("New Shader Graph.{0}", ShaderGraphImporter.Extension);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graph, format, null, null);
        }

        [MenuItem("Assets/Create/Shader/Hns17/CustomLit", false, 208)]
        public static void CreateCustomLitMaterialGraph()
        {
            var graph = CreateInstance<CreateCustomShaderGraph>();
            var format = string.Format("New Shader Graph.{0}", ShaderGraphImporter.Extension);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(1, graph, format, null, null);
        }


        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var graph = new GraphData();

            if(instanceId == 0)
                graph.AddNode(new LegacyMasterNode());
            else if(instanceId == 1)
                graph.AddNode(new CustomLitMasterNode());

            graph.path = "Shader Graphs";
            File.WriteAllText(pathName, EditorJsonUtility.ToJson(graph));
            AssetDatabase.Refresh();
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainMapGenerator))]
public class TerrainMapGeneratorEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        TerrainMapGenerator mapGenerator = (TerrainMapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdate)
            {
                mapGenerator.RenderMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.RenderMapInEditor();
        }
    }
}

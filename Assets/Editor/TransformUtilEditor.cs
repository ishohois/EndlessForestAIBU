using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransformUtils))]
public class TransformUtilEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TransformUtils transformUtil = (TransformUtils)target;

        if (GUILayout.Button("ResetChildren"))
        {
            transformUtil.ResetChildren();
        }
    }
}

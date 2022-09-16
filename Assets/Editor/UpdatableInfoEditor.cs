using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableInfo), true)]
public class UpdatableInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableInfo updatableInfo = (UpdatableInfo)target;

        if (GUILayout.Button("Update"))
        {
            updatableInfo.FireUpdatedInfoEvent();
        }
    }
}

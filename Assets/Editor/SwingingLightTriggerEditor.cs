using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SwingingLightTrigger), true)]
public class SwingingLightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        /*
        if (GUILayout.Button("Swing"))
        {
            (target as SwingingLightTrigger).swingEditor();
        }*/
    }
}

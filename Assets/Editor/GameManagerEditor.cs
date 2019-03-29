using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager), true)]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Screenshake"))
        {
            (target as GameManager).ShakeScreen(1);
        }
        if (GUILayout.Button("Progressive Screenshake"))
        {
            (target as GameManager).ProgressiveShake(1);
        }
        if (GUILayout.Button("Death"))
        {
            (target as GameManager).Death();
        }
    }
}

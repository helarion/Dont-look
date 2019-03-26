using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy),true)]
public class EnemyEditior : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Respawn"))
        {
            (target as Enemy).Respawn();
        }
    }
}

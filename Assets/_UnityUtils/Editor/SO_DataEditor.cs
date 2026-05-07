using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)]
public class SO_DataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // if(target is SO_ExamplesData soExamplesData){
        //     if (GUILayout.Button("EXAMPLE BUTTOn"))
        //     {
                
        //     }
        // }
    }
}

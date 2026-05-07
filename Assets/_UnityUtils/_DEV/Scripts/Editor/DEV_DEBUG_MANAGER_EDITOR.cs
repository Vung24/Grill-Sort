using UnityEditor;
using UnityEngine;

public partial class DEV_DEBUG_MANAGER_EDITOR : EditorWindow
{
	    [MenuItem("Unity Utils/Debug Manager")]
        public static void ShowWindow()
        {
            GetWindow<DEV_DEBUG_MANAGER_EDITOR>("DEV DEBUG MANAGER");
        }
    
        private void OnGUI()
        {
            GUILayout.Label("DEBUG SETTINGS", EditorStyles.boldLabel);
            
            DEV_DEBUG_MANAGER.ENABLE_LOG = EditorGUILayout.Toggle("ENABLE DEBUG LOG", DEV_DEBUG_MANAGER.ENABLE_LOG);
            DEV_DEBUG_MANAGER.ENABLE_TRIGGER_BOX_DRAW = EditorGUILayout.Toggle("ENABLE TRIGGER BOX DRAW", DEV_DEBUG_MANAGER.ENABLE_TRIGGER_BOX_DRAW);
            
            // Button Example
            if (GUILayout.Button("EXAMPLE BUTTON"))
            {
                
            }
        }
        
        
}

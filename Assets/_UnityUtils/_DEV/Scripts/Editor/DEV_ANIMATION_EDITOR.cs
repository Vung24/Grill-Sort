using System.Linq;
using UnityEditor;

[CustomEditor(typeof(DEV_LOCKED_ANIMATION))]
public class DEV_ANIMATION_EDITOR : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DEV_LOCKED_ANIMATION script = (DEV_LOCKED_ANIMATION)target;

        if (script.animator != null && script.animator.runtimeAnimatorController != null)
        {
            string[] clipNames = script.animator.runtimeAnimatorController.animationClips.Length > 0
                ? script.animator.runtimeAnimatorController.animationClips.Select(c => c.name).ToArray()
                : new string[0];
            
            script.selectedClipIndex = EditorGUILayout.Popup("Select Animation Clip", script.selectedClipIndex, clipNames);
        }
        else
        {
            EditorGUILayout.HelpBox("Animator or RuntimeAnimatorController is missing!", MessageType.Warning);
        }
    }
}

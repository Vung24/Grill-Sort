using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Utils_PrimeTween))]
public class Utils_PrimeTweenEditor : Editor {
	public override void OnInspectorGUI() {
#if PRIME_TWEEN
		serializedObject.Update();

		DrawProp("targets");
		DrawProp("tweenType");
		DrawProp("spaceType");
		DrawProp("playType");
		DrawProp("tweenStart");
		DrawProp("valueMode");

		var obj = (Utils_PrimeTween)target;

		if (obj != null) {
			var tweenTypeProp = serializedObject.FindProperty("tweenType");
			if (tweenTypeProp != null) {
				var tweenType = (Utils_PrimeTween.TweenType)tweenTypeProp.intValue;
				if (tweenType.HasFlag(Utils_PrimeTween.TweenType.Position))
					DrawProp("position", "Position Tween");

				if (tweenType.HasFlag(Utils_PrimeTween.TweenType.Rotation))
					DrawProp("rotation", "Rotation Tween");

				if (tweenType.HasFlag(Utils_PrimeTween.TweenType.Scale))
					DrawProp("scale", "Scale Tween");
			}

			var tweenStartProp = serializedObject.FindProperty("tweenStart");
			if (tweenStartProp != null) {
				var tweenStart = (Utils_PrimeTween.TweenStart)tweenStartProp.enumValueIndex;
				if (tweenStart == Utils_PrimeTween.TweenStart.Once) {
					DrawProp("delay", "Delay per Target");
					DrawProp("reverseOrder", "Reverse Order");
					DrawProp("loopYoYo", "Loop Yoyo");
				}
			}
		}

		GUILayout.Space(6);

		GUI.enabled = !Application.isPlaying;

		if (GUILayout.Button("Preview Tween"))
			obj.Play();

		if (GUILayout.Button("Stop"))
			obj.Stop();

		if (GUILayout.Button("Reset"))
			obj.ResetTransform();

		GUI.enabled = true;

		serializedObject.ApplyModifiedProperties();
#else
		EditorGUILayout.HelpBox("Define PRIME_TWEEN to use Utils_PrimeTween inspector controls.", MessageType.Warning);
		DrawDefaultInspector();
#endif
	}

	private void DrawProp(string name, string label = null) {
		var prop = serializedObject.FindProperty(name);
		if (prop == null)
			return;

		EditorGUILayout.PropertyField(
			prop,
			label == null ? null : new GUIContent(label),
			true
		);
	}
}
